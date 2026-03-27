using System.Data;
using Microsoft.Data.SqlClient;

namespace DashboardTsy.Infrastructure.Data;

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
            CommandType = CommandType.StoredProcedure,
            CommandTimeout = 180
        };

        if (parameters != null)
        {
            foreach (var p in parameters)
            {
                var name = p.Key.StartsWith("@") ? p.Key : "@" + p.Key;
                object? value = p.Value;

                if (value == null)
                {

                    value = DBNull.Value;
                }
                else if (value is string s && s.Trim().ToUpper() == "NULL")
                {

                    value = DBNull.Value;
                }
                else if (value is DateTime dt)
                {
                    if (dt <= (DateTime)System.Data.SqlTypes.SqlDateTime.MinValue || dt >= (DateTime)System.Data.SqlTypes.SqlDateTime.MaxValue)
                        value = DBNull.Value;
                }

                cmd.Parameters.AddWithValue(name, value);
            }
        }

        connection.Open();
        cmd.ExecuteNonQuery();
        using var adapter = new SqlDataAdapter(cmd);
        adapter.Fill(ds);

        return ds;
    }

    public DataSet ExecuteQueryDataSet(string connectionKey, string sql, IDictionary<string, object?>? parameters = null)
    {
        var connectionString = _connectionStrings.GetConnectionString(connectionKey);
        if (string.IsNullOrEmpty(connectionString))
            throw new InvalidOperationException($"Connection string not found for key: {connectionKey}.");

        var ds = new DataSet();
        using var connection = new SqlConnection(connectionString);
        using var cmd = new SqlCommand(sql, connection)
        {
            CommandType = CommandType.Text,
            CommandTimeout = 180
        };

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
}

