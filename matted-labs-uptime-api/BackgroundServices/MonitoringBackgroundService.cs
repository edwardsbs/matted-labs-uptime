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
        using var scope = scopeFactory.CreateScope();
        var serviceRepo = scope.ServiceProvider.GetRequiredService<IServiceRepository>();
        var checker = scope.ServiceProvider.GetRequiredService<IUptimeCheckerService>();
        var checkRepo = scope.ServiceProvider.GetRequiredService<IUptimeCheckRepository>();

        var services = await serviceRepo.GetAllAsync();
        var now = DateTime.UtcNow;

        var tasks = services
            .Where(s => s.IsActive)
            .Select(async service =>
            {
                var latest = await checkRepo.GetLatestForServiceAsync(service.Id);
                var nextCheck = latest?.CheckedAt.AddMinutes(service.IntervalMinutes) ?? DateTime.MinValue;

                if (now >= nextCheck)
                {
                    try { await checker.CheckServiceAsync(service); }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Error checking service {Name}", service.Name);
                    }
                }
            });

        await Task.WhenAll(tasks);

        // Daily prune
        try
        {
            await checkRepo.PruneOldChecksAsync();
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Prune failed");
        }
    }
}
