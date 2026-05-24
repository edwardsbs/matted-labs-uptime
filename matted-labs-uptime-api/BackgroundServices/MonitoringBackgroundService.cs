using MattedLabsUptime.Api.Repositories;
using MattedLabsUptime.Api.Services;

namespace MattedLabsUptime.Api.BackgroundServices;

public class MonitoringBackgroundService(
    IServiceScopeFactory scopeFactory,
    ILogger<MonitoringBackgroundService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Monitoring background service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            await RunChecksAsync(stoppingToken);
            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }

    private async Task RunChecksAsync(CancellationToken cancellationToken)
    {
        // Load the service list in its own short-lived scope
        IReadOnlyList<Api.Models.MonitoredService> services;
        using (var listScope = scopeFactory.CreateScope())
        {
            var serviceRepo = listScope.ServiceProvider.GetRequiredService<IServiceRepository>();
            services = (await serviceRepo.GetAllAsync()).Where(s => s.IsActive).ToList();
        }

        var now = DateTime.UtcNow;

        // Each service check gets its own scope so DbContext instances are never shared across threads
        var tasks = services.Select(async service =>
        {
            using var scope = scopeFactory.CreateScope();
            var checkRepo = scope.ServiceProvider.GetRequiredService<IUptimeCheckRepository>();
            var checker = scope.ServiceProvider.GetRequiredService<IUptimeCheckerService>();

            var latest = await checkRepo.GetLatestForServiceAsync(service.Id);
            var nextCheck = latest?.CheckedAt.AddMinutes(service.IntervalMinutes) ?? DateTime.MinValue;

            if (now < nextCheck) return;

            try { await checker.CheckServiceAsync(service); }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error checking service {Name}", service.Name);
            }
        });

        await Task.WhenAll(tasks);

        // Prune in its own scope too
        using var pruneScope = scopeFactory.CreateScope();
        try
        {
            var pruneRepo = pruneScope.ServiceProvider.GetRequiredService<IUptimeCheckRepository>();
            await pruneRepo.PruneOldChecksAsync();
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Prune failed");
        }
    }
}
