using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using MattedLabsUptime.Api.Models;

namespace MattedLabsUptime.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<MonitoredService> MonitoredServices => Set<MonitoredService>();
    public DbSet<UptimeCheck> UptimeChecks => Set<UptimeCheck>();
    public DbSet<AppSettings> AppSettings => Set<AppSettings>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MonitoredService>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).HasMaxLength(200).IsRequired();
            e.Property(x => x.Url).HasMaxLength(2000).IsRequired();
        });

        modelBuilder.Entity<AppSettings>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.SmtpHost).HasMaxLength(500);
            e.Property(x => x.SmtpUser).HasMaxLength(500);
            e.Property(x => x.SmtpPassword).HasMaxLength(500);
            e.Property(x => x.SmtpFrom).HasMaxLength(500);
            e.Property(x => x.AlertRecipient).HasMaxLength(500);
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

        // EF Core reads datetime2 from SQL Server as Unspecified kind — force UTC so
        // System.Text.Json serializes with a Z suffix and Angular converts correctly.
        var utcConverter = new ValueConverter<DateTime, DateTime>(
            v => v,
            v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            foreach (var property in entityType.GetProperties())
                if (property.ClrType == typeof(DateTime))
                    property.SetValueConverter(utcConverter);
    }
}
