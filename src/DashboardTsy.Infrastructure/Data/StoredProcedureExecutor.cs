using System.Collections.Concurrent;
using System.Data;
using Microsoft.Data.SqlClient;

namespace DashboardTsy.Infrastructure.Data;

public class StoredProcedureExecutor : IStoredProcedureExecutor
{
    // Sağlıksız / takılmış SP çağrılarının thread'i uzun süre işgal etmemesi için savunma değeri.
    // ADO.NET default'u 30sn, biz de aynı tabana oturuyoruz; çağıran taraf gerekirse override edebilir.
    private const int DefaultCommandTimeoutSeconds = 30;

    private readonly IConnectionStringProvider _connectionStrings;

    // SP imzalarını (parametre adı + SqlDbType + size + precision + scale + direction) process boyu cache'ler.
    // Key: connectionKey|procedureName (case-insensitive).
    // Bir SP ilk çağrıldığında DeriveParameters ile SQL'den okunur; sonraki çağrılarda cache'den klonlanır.
    // Bu, ADO.NET'in AddWithValue ile nvarchar(4000) göndermesini engeller — execution plan sapmasını önler.
    private static readonly ConcurrentDictionary<string, SqlParameter[]> ParameterSchemaCache
        = new(StringComparer.OrdinalIgnoreCase);

    public StoredProcedureExecutor(IConnectionStringProvider connectionStrings)
    {
        _connectionStrings = connectionStrings;
    }

    public DataSet ExecuteDataSet(
        string connectionKey,
        string procedureName,
        IDictionary<string, object?>? parameters = null,
        int? commandTimeoutSeconds = null)
    {
        var connectionString = _connectionStrings.GetConnectionString(connectionKey);
        if (string.IsNullOrEmpty(connectionString))
            throw new InvalidOperationException($"Connection string not found for key: {connectionKey}.");

        var ds = new DataSet();
        using var connection = new SqlConnection(connectionString);
        using var cmd = new SqlCommand(procedureName, connection)
        {
            CommandType = CommandType.StoredProcedure,
            CommandTimeout = commandTimeoutSeconds ?? DefaultCommandTimeoutSeconds
        };

        connection.Open();

        BindParameters(cmd, connectionKey, procedureName, parameters);

        using var adapter = new SqlDataAdapter(cmd);
        adapter.Fill(ds);

        return ds;
    }

    public DataSet ExecuteQueryDataSet(
        string connectionKey,
        string sql,
        IDictionary<string, object?>? parameters = null,
        int? commandTimeoutSeconds = null)
    {
        var connectionString = _connectionStrings.GetConnectionString(connectionKey);
        if (string.IsNullOrEmpty(connectionString))
            throw new InvalidOperationException($"Connection string not found for key: {connectionKey}.");

        var ds = new DataSet();
        using var connection = new SqlConnection(connectionString);
        using var cmd = new SqlCommand(sql, connection)
        {
            CommandType = CommandType.Text,
            CommandTimeout = commandTimeoutSeconds ?? DefaultCommandTimeoutSeconds
        };

        // Ad-hoc SQL için DeriveParameters uygulanamaz (SP değil). Mevcut AddWithValue davranışı korunur.
        if (parameters != null)
        {
            foreach (var p in parameters)
            {
                var name = p.Key.StartsWith("@") ? p.Key : "@" + p.Key;
                cmd.Parameters.AddWithValue(name, p.Value ?? DBNull.Value);
            }
        }

        connection.Open();
        using var adapter = new SqlDataAdapter(cmd);
        adapter.Fill(ds);

        return ds;
    }

    /// <summary>
    /// SP'nin gerçek parametre imzasını SQL'den (bir kez) çeker, cache'ler ve doğru tiple değer bind eder.
    /// DeriveParameters başarısız olursa (ör. yetki yok) eski AddWithValue davranışına fallback yapar.
    /// </summary>
    private static void BindParameters(
        SqlCommand cmd,
        string connectionKey,
        string procedureName,
        IDictionary<string, object?>? parameters)
    {
        var schema = TryGetSchema(cmd, connectionKey, procedureName);

        if (schema is null)
        {
            // Fallback: SP imzası çözülemedi, eski davranışa dön (davranış aynı, sadece tip optimizasyonu yok).
            BindWithAddWithValue(cmd, parameters);
            return;
        }

        // Şablonu klonlayarak command'a bağla. Cache'teki SqlParameter'lara doğrudan dokunmuyoruz — thread-safe.
        foreach (var template in schema)
        {
            var clone = CloneParameter(template);
            cmd.Parameters.Add(clone);
        }

        if (parameters == null) return;

        // Verilen değerleri normalize edip ilgili parametreye ata. Bilinmeyen anahtarlar sessizce yok sayılır
        // (mevcut davranışı korumak için — SP kaldırılmış bir parametre bekliyorsa da patlamayız).
        foreach (var p in parameters)
        {
            var name = p.Key.StartsWith("@") ? p.Key : "@" + p.Key;
            if (!cmd.Parameters.Contains(name)) continue;

            cmd.Parameters[name].Value = NormalizeValue(p.Value);
        }
    }

    private static SqlParameter[]? TryGetSchema(SqlCommand cmd, string connectionKey, string procedureName)
    {
        var cacheKey = $"{connectionKey}|{procedureName}";

        if (ParameterSchemaCache.TryGetValue(cacheKey, out var cached))
            return cached;

        try
        {
            // DeriveParameters: SP tanımını SQL'den okuyup cmd.Parameters'ı doldurur. Bir ekstra roundtrip.
            SqlCommandBuilder.DeriveParameters(cmd);

            // İlk parametre (@RETURN_VALUE) SP'nin dönüş değeri için — bind etmemiz gerekiyor, atmıyoruz.
            var derived = new SqlParameter[cmd.Parameters.Count];
            for (int i = 0; i < cmd.Parameters.Count; i++)
                derived[i] = CloneParameter(cmd.Parameters[i]);

            // Cache'ledikten sonra cmd.Parameters'ı temizle — çağıran taraf klonları tekrar ekleyecek.
            cmd.Parameters.Clear();

            ParameterSchemaCache[cacheKey] = derived;
            return derived;
        }
        catch
        {
            // Yetki yok, SP bulunamadı vb. — fallback için null döneriz. Bir sonraki çağrıda tekrar denenmesin
            // diye null'ı da cache'lemek isterdik ama ConcurrentDictionary null value tutmaz; ilk hatadan sonra
            // her çağrıda küçük bir tekrar deneme olur. Yaygın senaryoda bu yol hiç tetiklenmez.
            cmd.Parameters.Clear();
            return null;
        }
    }

    private static SqlParameter CloneParameter(SqlParameter source)
    {
        var clone = new SqlParameter
        {
            ParameterName = source.ParameterName,
            SqlDbType = source.SqlDbType,
            Size = source.Size,
            Precision = source.Precision,
            Scale = source.Scale,
            Direction = source.Direction,
            IsNullable = source.IsNullable
        };

        if (source.SqlDbType == SqlDbType.Udt)
            clone.UdtTypeName = source.UdtTypeName;
        if (source.SqlDbType == SqlDbType.Structured)
            clone.TypeName = source.TypeName;

        // Return-value parametrelerini default DBNull ile başlat; input'lar zaten dışarıdan set edilecek.
        clone.Value = DBNull.Value;
        return clone;
    }

    private static object NormalizeValue(object? value)
    {
        if (value is null)
            return DBNull.Value;

        if (value is string s && s.Trim().Equals("NULL", StringComparison.OrdinalIgnoreCase))
            return DBNull.Value;

        if (value is DateTime dt)
        {
            if (dt <= (DateTime)System.Data.SqlTypes.SqlDateTime.MinValue ||
                dt >= (DateTime)System.Data.SqlTypes.SqlDateTime.MaxValue)
                return DBNull.Value;
        }

        return value;
    }

    private static void BindWithAddWithValue(SqlCommand cmd, IDictionary<string, object?>? parameters)
    {
        if (parameters == null) return;

        foreach (var p in parameters)
        {
            var name = p.Key.StartsWith("@") ? p.Key : "@" + p.Key;
            cmd.Parameters.AddWithValue(name, NormalizeValue(p.Value));
        }
    }
}
