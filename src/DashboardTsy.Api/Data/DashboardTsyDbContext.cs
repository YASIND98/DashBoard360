using Microsoft.EntityFrameworkCore;

namespace DashboardTsy.Api.Data;

public class DashboardTsyDbContext : DbContext
{
    public DashboardTsyDbContext(DbContextOptions<DashboardTsyDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(e =>
        {
            e.ToTable("Users");
            e.HasKey(x => x.UserId);
            e.Property(x => x.Password).HasMaxLength(500);
        });
    }
}
