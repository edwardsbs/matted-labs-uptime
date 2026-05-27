using MattedLabsUptime.Api.Models;

namespace MattedLabsUptime.Api.Repositories;

public interface ISettingsRepository
{
    Task<AppSettings> GetAsync();
    Task UpdateAsync(AppSettings settings);
}
