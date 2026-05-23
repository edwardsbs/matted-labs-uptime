using System.Diagnostics;
using MattedLabsUptime.Api.Models;
using MattedLabsUptime.Api.Repositories;

namespace MattedLabsUptime.Api.Services;

public interface IUptimeCheckerService
{
    Task CheckServiceAsync(MonitoredService service);
}

public class UptimeCheckerService(
    IUptimeCheckRepository checkRepo,
    IEmailService emailService,
    IHttpClientFactory httpClientFactory,
    ILogger<UptimeCheckerService> logger) : IUptimeCheckerService
{
    // Track last-known state per service to avoid duplicate alerts
    private static readonly Dictionary<int, bool> _lastState = new();

    public async Task CheckServiceAsync(MonitoredService service)
    {
        var sw = Stopwatch.StartNew();
        bool isUp = false;
        int? statusCode = null;
        string? errorMessage = null;

        try
        {
            var clientName = service.IgnoreSslErrors ? "IgnoreSsl" : "Default";
            var client = httpClientFactory.CreateClient(clientName);
            var response = await client.GetAsync(service.Url);
            sw.Stop();

            statusCode = (int)response.StatusCode;
            isUp = response.IsSuccessStatusCode;
            if (!isUp)
                errorMessage = $"HTTP {statusCode}";
        }
        catch (Exception ex)
        {
            sw.Stop();
            errorMessage = ex.Message.Length > 500 ? ex.Message[..500] : ex.Message;
        }

        var check = new UptimeCheck
        {
            MonitoredServiceId = service.Id,
            CheckedAt = DateTime.UtcNow,
            IsUp = isUp,
            ResponseTimeMs = sw.ElapsedMilliseconds,
            StatusCode = statusCode,
            ErrorMessage = errorMessage
        };

        await checkRepo.AddAsync(check);
        logger.LogInformation("[{Name}] {Status} {ResponseMs}ms", service.Name, isUp ? "UP" : "DOWN", sw.ElapsedMilliseconds);

        await HandleAlertAsync(service, isUp, errorMessage);
    }

    private async Task HandleAlertAsync(MonitoredService service, bool isUp, string? error)
    {
        _lastState.TryGetValue(service.Id, out var previouslyUp);

        if (!isUp && previouslyUp)
            await emailService.SendDownAlertAsync(service.Name, service.Url, error);
        else if (isUp && !previouslyUp && _lastState.ContainsKey(service.Id))
        {
            var latest = await checkRepo.GetLatestForServiceAsync(service.Id);
            await emailService.SendRecoveryAlertAsync(service.Name, service.Url, latest?.ResponseTimeMs ?? 0);
        }

        _lastState[service.Id] = isUp;
    }
}
