using System.Data;

namespace DashboardTsy.Infrastructure.Data;

public interface IStoredProcedureExecutor
{
    /// <summary>
    /// Bir stored procedure'u çalıştırıp sonuç setini DataSet olarak döner.
    /// </summary>
    /// <param name="commandTimeoutSeconds">
    /// SqlCommand.CommandTimeout değeri (saniye). Belirtilmezse varsayılan (30sn) kullanılır.
    /// Bilinen yavaş raporlar için özel değer verilebilir; sıfır = sınırsız (önerilmez).
    /// </param>
    DataSet ExecuteDataSet(
        string connectionKey,
        string procedureName,
        IDictionary<string, object?>? parameters = null,
        int? commandTimeoutSeconds = null);

    DataSet ExecuteQueryDataSet(
        string connectionKey,
        string sql,
        IDictionary<string, object?>? parameters = null,
        int? commandTimeoutSeconds = null);
}

