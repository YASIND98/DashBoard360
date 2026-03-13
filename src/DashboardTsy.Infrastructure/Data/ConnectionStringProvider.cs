using Microsoft.Extensions.Configuration;

namespace DashboardTsy.Infrastructure.Data;

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

