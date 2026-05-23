using Microsoft.EntityFrameworkCore;
using MattedLabsUptime.Api.Data;
using MattedLabsUptime.Api.Models;

namespace MattedLabsUptime.Api.Repositories;

public class UptimeCheckRepository(AppDbContext db) : IUptimeCheckRepository
{
    public async Task<UptimeCheck> AddAsync(UptimeCheck check)
    {
        db.UptimeChecks.Add(check);
        await db.SaveChangesAsync();
        return check;
    }

    public async Task<UptimeCheck?> GetLatestForServiceAsync(int serviceId) =>
        await db.UptimeChecks
            .Where(c => c.MonitoredServiceId == serviceId)
            .OrderByDescending(c => c.CheckedAt)
            .FirstOrDefaultAsync();

    public async Task<IEnumerable<UptimeCheck>> GetForServiceAsync(int serviceId, int limit = 100) =>
        await db.UptimeChecks
            .Where(c => c.MonitoredServiceId == serviceId)
            .OrderByDescending(c => c.CheckedAt)
            .Take(limit)
            .ToListAsync();

    public async Task<IEnumerable<UptimeCheck>> GetSinceAsync(int serviceId, DateTime since) =>
        await db.UptimeChecks
            .Where(c => c.MonitoredServiceId == serviceId && c.CheckedAt >= since)
            .OrderByDescending(c => c.CheckedAt)
            .ToListAsync();

    public async Task PruneOldChecksAsync(int keepDays = 90)
    {
        var cutoff = DateTime.UtcNow.AddDays(-keepDays);
        await db.UptimeChecks
            .Where(c => c.CheckedAt < cutoff)
            .ExecuteDeleteAsync();
    }
}
