using Microsoft.EntityFrameworkCore;
using MattedLabsUptime.Api.Data;
using MattedLabsUptime.Api.Models;

namespace MattedLabsUptime.Api.Repositories;

public class SettingsRepository(AppDbContext db) : ISettingsRepository
{
    public async Task<AppSettings> GetAsync() =>
        await db.AppSettings.FirstOrDefaultAsync() ?? new AppSettings();

    public async Task UpdateAsync(AppSettings settings)
    {
        db.AppSettings.Update(settings);
        await db.SaveChangesAsync();
    }
}
