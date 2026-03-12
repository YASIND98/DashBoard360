using System.Data;

namespace DashboardTsy.Api.Services;

/// <summary>
/// Executes SQL Server stored procedures and returns DataSet (DBRapor DataLayer pattern).
/// </summary>
public interface IStoredProcedureExecutor
{
    /// <summary>
    /// Executes a stored procedure and returns the result as DataSet (multiple result sets supported).
    /// </summary>
    /// <param name="connectionKey">Key for connection string (e.g. Main, SubeDashboard, PMO).</param>
    /// <param name="procedureName">Stored procedure name (e.g. sp_GetRaporTarihi).</param>
    /// <param name="parameters">Optional parameters: parameter name (with or without @) and value.</param>
    DataSet ExecuteDataSet(string connectionKey, string procedureName, IDictionary<string, object?>? parameters = null);
}
