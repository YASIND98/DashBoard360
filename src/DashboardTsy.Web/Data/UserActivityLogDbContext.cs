using Microsoft.EntityFrameworkCore;

namespace DashboardTsy.Web.Data;

/// <summary>
/// Tablo DB'de elle oluşturulur (<c>Scripts/UserActivityLogs.sql</c>); migration yok.
/// </summary>
public class UserActivityLogDbContext : DbContext
{
    public UserActivityLogDbContext(DbContextOptions<UserActivityLogDbContext> options)
        : base(options)
    {
    }

    public DbSet<UserActivityLog> UserActivityLogs => Set<UserActivityLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserActivityLog>(e =>
        {
            e.ToTable("UserActivityLogs");
            e.HasKey(x => x.Id);
            e.Property(x => x.EventType).HasMaxLength(64).IsRequired();
            e.Property(x => x.UserDisplayName).HasMaxLength(200);
            e.Property(x => x.ActionName).HasMaxLength(500);
            e.Property(x => x.Route).HasMaxLength(500);
            e.Property(x => x.PageUrl).HasMaxLength(2000);
            e.Property(x => x.IpAddress).HasMaxLength(45);
            e.Property(x => x.UserAgent).HasMaxLength(500);
            e.Property(x => x.CreatedAtUtc).IsRequired();
        });
    }
}
