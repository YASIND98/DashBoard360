namespace DashboardTsy.Api.Services;

/// <summary>
/// Provides named connection strings (e.g. Main, SubeDashboard, PMO) for DataLayer / stored procedure execution.
/// Mirrors DBRapor's Mutuals.DbConnStrings usage.
/// </summary>
public interface IConnectionStringProvider
{
    string? GetConnectionString(string key);
}
