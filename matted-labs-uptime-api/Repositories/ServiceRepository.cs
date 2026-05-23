using Microsoft.EntityFrameworkCore;
using MattedLabsUptime.Api.Data;
using MattedLabsUptime.Api.Models;

namespace MattedLabsUptime.Api.Repositories;

public class ServiceRepository(AppDbContext db) : IServiceRepository
{
    public async Task<IEnumerable<MonitoredService>> GetAllAsync() =>
        await db.MonitoredServices.OrderBy(s => s.Name).ToListAsync();

    public async Task<MonitoredService?> GetByIdAsync(int id) =>
        await db.MonitoredServices.FindAsync(id);

    public async Task<MonitoredService> CreateAsync(MonitoredService service)
    {
        db.MonitoredServices.Add(service);
        await db.SaveChangesAsync();
        return service;
    }

    public async Task<MonitoredService?> UpdateAsync(int id, MonitoredService updated)
    {
        var existing = await db.MonitoredServices.FindAsync(id);
        if (existing is null) return null;

        existing.Name = updated.Name;
        existing.Url = updated.Url;
        existing.IsActive = updated.IsActive;
        existing.IgnoreSslErrors = updated.IgnoreSslErrors;
        existing.IntervalMinutes = updated.IntervalMinutes;

        await db.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var existing = await db.MonitoredServices.FindAsync(id);
        if (existing is null) return false;

        db.MonitoredServices.Remove(existing);
        await db.SaveChangesAsync();
        return true;
    }
}
