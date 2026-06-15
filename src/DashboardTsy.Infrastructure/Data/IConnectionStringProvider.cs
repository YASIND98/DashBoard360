namespace DashboardTsy.Infrastructure.Data;

public interface IConnectionStringProvider
{
    string? GetConnectionString(string key);
}

