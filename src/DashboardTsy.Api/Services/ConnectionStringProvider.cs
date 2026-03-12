namespace DashboardTsy.Api.Services;

/// <summary>
/// Reads connection strings from configuration (DbConnectionStrings:Key or ConnectionStrings:Key).
/// </summary>
public class ConnectionStringProvider : IConnectionStringProvider
{
    private readonly IConfiguration _configuration;

    public ConnectionStringProvider(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string? GetConnectionString(string key)
    {
        return _configuration["DbConnectionStrings:" + key]
               ?? _configuration.GetConnectionString(key);
    }
}
