using Microsoft.EntityFrameworkCore;
using MattedLabsUptime.Api.Models;

namespace MattedLabsUptime.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<MonitoredService> MonitoredServices => Set<MonitoredService>();
    public DbSet<UptimeCheck> UptimeChecks => Set<UptimeCheck>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MonitoredService>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).HasMaxLength(200).IsRequired();
            e.Property(x => x.Url).HasMaxLength(2000).IsRequired();
        });

        modelBuilder.Entity<UptimeCheck>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.CheckedAt);
            e.HasIndex(x => x.MonitoredServiceId);
            e.Property(x => x.ErrorMessage).HasMaxLength(2000);
            e.HasOne(x => x.MonitoredService)
             .WithMany(x => x.UptimeChecks)
             .HasForeignKey(x => x.MonitoredServiceId)
             .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
