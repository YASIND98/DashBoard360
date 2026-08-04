using System.Data;
using System.Text.Json;

namespace DashboardTsy.Infrastructure.NplReport;

/// <summary>
/// SP_RP_NPL_Bakiye_Oran'ın kabul ettiği parametrelerin allow-list'i ve tip meta'sı.
/// Frontend'den gelen key/value ikilileri buradaki tanıma göre doğrulanır ve
/// hedef SQL tipine dönüştürülür. Listede olmayan parametreler reddedilir.
/// </summary>
public static class NplBalanceRatioSpParameters
{
    public sealed record Spec(string Name, SqlDbType SqlType);

    public static readonly IReadOnlyDictionary<string, Spec> AllowedParameters =
        new Dictionary<string, Spec>(StringComparer.OrdinalIgnoreCase)
        {
            ["@KAT_DONEM"] = new("@KAT_DONEM", SqlDbType.VarChar),
            ["@ISKOLU"] = new("@ISKOLU", SqlDbType.VarChar),
            ["@TAHSIS_KOLU"] = new("@TAHSIS_KOLU", SqlDbType.VarChar),
            ["@SUBE_KODU"] = new("@SUBE_KODU", SqlDbType.Int),
            ["@BOLGE_KODU"] = new("@BOLGE_KODU", SqlDbType.Int),
            ["@URUN"] = new("@URUN", SqlDbType.VarChar),
            ["@Yıl"] = new("@Yıl", SqlDbType.Int),
            ["@YETKI_KODU"] = new("@YETKI_KODU", SqlDbType.Char),
            ["@BONUS_BUSINESS_FLAG"] = new("@BONUS_BUSINESS_FLAG", SqlDbType.Bit),
            ["@BIREYSEL_MIKRO_FLAG"] = new("@BIREYSEL_MIKRO_FLAG", SqlDbType.Bit),
            ["@IRS_Flag"] = new("@IRS_Flag", SqlDbType.Int),
            ["@YAPILANDIRMA_FLAG"] = new("@YAPILANDIRMA_FLAG", SqlDbType.Bit),
            ["@YAPILANDIRMA_FLAG_KREDI"] = new("@YAPILANDIRMA_FLAG_KREDI", SqlDbType.VarChar),
            ["@IHTIYAC_TICARI_FLAG"] = new("@IHTIYAC_TICARI_FLAG", SqlDbType.Bit),
            ["@KGFLI_KREDI_FLAG"] = new("@KGFLI_KREDI_FLAG", SqlDbType.Bit),
            ["@KGFLI_MUST_FLAG"] = new("@KGFLI_MUST_FLAG", SqlDbType.Bit),
            ["@EMEKLI_FLAG"] = new("@EMEKLI_FLAG", SqlDbType.Bit),
            ["@DB_MAAS_ODEMESI_FLAG"] = new("@DB_MAAS_ODEMESI_FLAG", SqlDbType.Bit),
            ["@OB"] = new("@OB", SqlDbType.VarChar)
        };

    /// <summary>
    /// Frontend'den gelen ham key/value sözlüğünü SP'ye gönderilecek forma
    /// çevirir. Allow-list dışı anahtarlar sessizce atlanır. Değer null veya
    /// boş string ise DBNull üretilir.
    /// </summary>
    public static Dictionary<string, object?> Bind(IDictionary<string, object?>? incoming)
    {
        var result = new Dictionary<string, object?>();
        if (incoming == null) return result;

        foreach (var kv in incoming)
        {
            var key = NormalizeKey(kv.Key);
            if (!AllowedParameters.TryGetValue(key, out var spec)) continue;

            result[spec.Name] = Coerce(kv.Value, spec.SqlType);
        }

        return result;
    }

    private static string NormalizeKey(string raw)
        => string.IsNullOrEmpty(raw) ? string.Empty : (raw.StartsWith("@") ? raw : "@" + raw);

    private static object? Coerce(object? value, SqlDbType targetType)
    {
        if (value is null) return null;

        if (value is JsonElement je)
        {
            if (je.ValueKind == JsonValueKind.Null || je.ValueKind == JsonValueKind.Undefined)
                return null;

            switch (targetType)
            {
                case SqlDbType.Bit:
                    if (je.ValueKind == JsonValueKind.True) return true;
                    if (je.ValueKind == JsonValueKind.False) return false;
                    if (je.ValueKind == JsonValueKind.Number && je.TryGetInt32(out var bn)) return bn != 0;
                    if (je.ValueKind == JsonValueKind.String && bool.TryParse(je.GetString(), out var bs)) return bs;
                    return null;

                case SqlDbType.Int:
                    if (je.ValueKind == JsonValueKind.Number && je.TryGetInt32(out var i)) return i;
                    if (je.ValueKind == JsonValueKind.String && int.TryParse(je.GetString(), out var ip)) return ip;
                    return null;

                case SqlDbType.VarChar:
                case SqlDbType.Char:
                case SqlDbType.NVarChar:
                    var s = je.ValueKind == JsonValueKind.String ? je.GetString() : je.ToString();
                    return string.IsNullOrWhiteSpace(s) ? null : s;

                default:
                    return je.ToString();
            }
        }

        if (value is string str)
            return string.IsNullOrWhiteSpace(str) ? null : str;

        return value;
    }
}
