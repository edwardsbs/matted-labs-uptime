using Microsoft.EntityFrameworkCore;
using MattedLabsUptime.Api.Models;

namespace MattedLabsUptime.Api.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext db)
    {
        await db.Database.MigrateAsync();

        if (await db.MonitoredServices.AnyAsync())
            return;

        var services = new List<MonitoredService>
        {
            new() { Name = "Bartender API",       Url = "http://bartender-api:8080/health",  IntervalMinutes = 5 },
            new() { Name = "Bartender Web",        Url = "http://bartender-web:4200",          IntervalMinutes = 5 },
            new() { Name = "Out The Door",         Url = "http://out-the-door:3000",           IntervalMinutes = 5 },
            new() { Name = "Proxmox Dashboard",    Url = "https://192.168.0.7:8006",           IntervalMinutes = 5, IgnoreSslErrors = true },
            new() { Name = "Ledger API",           Url = "http://ledger-api:5000/health",      IntervalMinutes = 5 },
            new() { Name = "Ledger Web",           Url = "http://ledger-web:4200",             IntervalMinutes = 5 },
        };

        db.MonitoredServices.AddRange(services);
        await db.SaveChangesAsync();
    }
}
