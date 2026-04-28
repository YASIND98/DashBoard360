using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DashboardTsy.Web.DataProtection;

public sealed class DashboardTsyDataProtectionKeyContext : DbContext, IDataProtectionKeyContext
{
    public DashboardTsyDataProtectionKeyContext(DbContextOptions<DashboardTsyDataProtectionKeyContext> options)
        : base(options)
    {
    }

    public DbSet<DataProtectionKey> DataProtectionKeys { get; set; } = null!;
}

