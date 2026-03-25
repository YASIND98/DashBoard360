using System.Data;

namespace DashboardTsy.Infrastructure.Data;

public interface IStoredProcedureExecutor
{
    DataSet ExecuteDataSet(string connectionKey, string procedureName, IDictionary<string, object?>? parameters = null);

    DataSet ExecuteQueryDataSet(string connectionKey, string sql, IDictionary<string, object?>? parameters = null);
}

