using MattedLabsUptime.Api.Models;

namespace MattedLabsUptime.Api.Repositories;

public interface IServiceRepository
{
    Task<IEnumerable<MonitoredService>> GetAllAsync();
    Task<MonitoredService?> GetByIdAsync(int id);
    Task<MonitoredService> CreateAsync(MonitoredService service);
    Task<MonitoredService?> UpdateAsync(int id, MonitoredService updated);
    Task<bool> DeleteAsync(int id);
}
