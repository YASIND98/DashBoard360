using DashboardTsy.Application.AppSettings;
using DashboardTsy.Domain.AppSettings;
using DashboardTsy.Infrastructure.Data;

namespace DashboardTsy.Infrastructure.AppSettings;

public sealed class AppSettingsRepository : IAppSettingsRepository
{
    private const string ConnectionKey = "YoneticiRaporu";
    private const string Sql = @"SELECT [Key], [Value], [ValueType], [Description] FROM dbo.AppSettings WHERE [IsActive] = 1";

    private readonly IStoredProcedureExecutor _executor;

    public AppSettingsRepository(IStoredProcedureExecutor executor)
    {
        _executor = executor;
    }

    public IReadOnlyList<AppSetting> GetAll()
    {
        var ds = _executor.ExecuteQueryDataSet(ConnectionKey, Sql);
        if (ds.Tables.Count == 0) return Array.Empty<AppSetting>();

        var rows = ds.Tables[0].Rows;
        var list = new List<AppSetting>(rows.Count);
        foreach (System.Data.DataRow row in rows)
        {
            var key = row["Key"] as string ?? string.Empty;
            var value = row["Value"] == DBNull.Value ? null : row["Value"]?.ToString();
            var typeRaw = Convert.ToInt32(row["ValueType"]);
            var description = row["Description"] == DBNull.Value ? null : row["Description"]?.ToString();
            list.Add(new AppSetting(key, value, (AppSettingType)typeRaw, description));
        }
        return list;
    }
}
