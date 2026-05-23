using MattedLabsUptime.Api.Models;

namespace MattedLabsUptime.Api.Repositories;

public interface IUptimeCheckRepository
{
    Task<UptimeCheck> AddAsync(UptimeCheck check);
    Task<UptimeCheck?> GetLatestForServiceAsync(int serviceId);
    Task<IEnumerable<UptimeCheck>> GetForServiceAsync(int serviceId, int limit = 100);
    Task<IEnumerable<UptimeCheck>> GetSinceAsync(int serviceId, DateTime since);
    Task PruneOldChecksAsync(int keepDays = 90);
}
