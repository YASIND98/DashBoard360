using System.Data;
using Microsoft.Data.SqlClient;

namespace DashboardTsy.Api.Services;

/// <summary>
/// Executes stored procedures using SqlConnection + SqlCommand(CommandType.StoredProcedure) + SqlDataAdapter.Fill (DBRapor pattern).
/// </summary>
public class StoredProcedureExecutor : IStoredProcedureExecutor
{
    private readonly IConnectionStringProvider _connectionStrings;

    public StoredProcedureExecutor(IConnectionStringProvider connectionStrings)
    {
        _connectionStrings = connectionStrings;
    }

    public DataSet ExecuteDataSet(string connectionKey, string procedureName, IDictionary<string, object?>? parameters = null)
    {
        var connectionString = _connectionStrings.GetConnectionString(connectionKey);
        if (string.IsNullOrEmpty(connectionString))
            throw new InvalidOperationException($"Connection string not found for key: {connectionKey}.");

        var ds = new DataSet();
        using var connection = new SqlConnection(connectionString);
        using var cmd = new SqlCommand(procedureName, connection)
        {
            CommandType = CommandType.StoredProcedure
        };

        if (parameters != null)
        {
            foreach (var p in parameters)
            {
                var name = p.Key.StartsWith("@") ? p.Key : "@" + p.Key;
                var value = p.Value ?? DBNull.Value;
                cmd.Parameters.AddWithValue(name, value);
            }
        }

        connection.Open();
        cmd.ExecuteNonQuery();
        using var adapter = new SqlDataAdapter(cmd);
        adapter.Fill(ds);

        return ds;
    }
}
