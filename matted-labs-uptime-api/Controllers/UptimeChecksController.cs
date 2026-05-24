using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using MattedLabsUptime.Api.Models;
using MattedLabsUptime.Api.Repositories;
using MattedLabsUptime.Api.Services;

namespace MattedLabsUptime.Api.Controllers;

[ApiController]
[Route("api/checks")]
public class UptimeChecksController(
    IUptimeCheckRepository checkRepo,
    IServiceRepository serviceRepo,
    IUptimeCheckerService checker,
    IHttpClientFactory httpClientFactory) : ControllerBase
{
    [HttpGet("{serviceId:int}")]
    public async Task<IActionResult> GetHistory(int serviceId, [FromQuery] int limit = 100)
    {
        var checks = await checkRepo.GetForServiceAsync(serviceId, limit);
        return Ok(checks.Select(ToDto));
    }

    [HttpGet("{serviceId:int}/stats")]
    public async Task<IActionResult> GetStats(int serviceId)
    {
        var now = DateTime.UtcNow;
        var c24 = await checkRepo.GetSinceAsync(serviceId, now.AddHours(-24));
        var c7d = await checkRepo.GetSinceAsync(serviceId, now.AddDays(-7));
        var c30d = await checkRepo.GetSinceAsync(serviceId, now.AddDays(-30));

        return Ok(new UptimeStatsDto(
            Uptime24h: CalcUptime(c24),
            Uptime7d: CalcUptime(c7d),
            Uptime30d: CalcUptime(c30d)
        ));
    }

    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard()
    {
        var services = await serviceRepo.GetAllAsync();
        var now = DateTime.UtcNow;

        var results = new List<ServiceStatusDto>();
        foreach (var s in services)
        {
            var latest = await checkRepo.GetLatestForServiceAsync(s.Id);
            var recent = await checkRepo.GetSinceAsync(s.Id, now.AddHours(-24));
            var c7d = await checkRepo.GetSinceAsync(s.Id, now.AddDays(-7));
            var c30d = await checkRepo.GetSinceAsync(s.Id, now.AddDays(-30));
            var sparkline = await checkRepo.GetForServiceAsync(s.Id, 48);

            results.Add(new ServiceStatusDto(
                Service: new(s.Id, s.Name, s.Url, s.IsActive, s.IgnoreSslErrors, s.IntervalMinutes, s.CreatedAt),
                LatestCheck: latest is null ? null : ToDto(latest),
                Uptime24h: CalcUptime(recent),
                Uptime7d: CalcUptime(c7d),
                Uptime30d: CalcUptime(c30d),
                RecentChecks: sparkline.Select(ToDto)
            ));
        }

        return Ok(results);
    }

    [HttpPost("{serviceId:int}/check-now")]
    public async Task<IActionResult> CheckNow(int serviceId)
    {
        var service = await serviceRepo.GetByIdAsync(serviceId);
        if (service is null) return NotFound();
        await checker.CheckServiceAsync(service);
        var latest = await checkRepo.GetLatestForServiceAsync(serviceId);
        return Ok(latest is null ? null : ToDto(latest));
    }

    [HttpPost("test")]
    public async Task<IActionResult> TestUrl([FromBody] TestUrlRequest request)
    {
        var clientName = request.IgnoreSslErrors ? "IgnoreSsl" : "Default";
        var client = httpClientFactory.CreateClient(clientName);
        var sw = Stopwatch.StartNew();
        bool isUp = false;
        int? statusCode = null;
        string? errorMessage = null;

        try
        {
            var response = await client.GetAsync(request.Url);
            sw.Stop();
            statusCode = (int)response.StatusCode;
            isUp = response.IsSuccessStatusCode;
            if (!isUp) errorMessage = $"HTTP {statusCode}";
        }
        catch (Exception ex)
        {
            sw.Stop();
            errorMessage = ex.Message.Length > 300 ? ex.Message[..300] : ex.Message;
        }

        return Ok(new TestUrlResult(isUp, sw.ElapsedMilliseconds, statusCode, errorMessage));
    }

    private static double CalcUptime(IEnumerable<UptimeCheck> checks)
    {
        var list = checks.ToList();
        if (list.Count == 0) return 100.0;
        return Math.Round(list.Count(c => c.IsUp) * 100.0 / list.Count, 2);
    }

    private static UptimeCheckDto ToDto(UptimeCheck c) =>
        new(c.Id, c.MonitoredServiceId, c.CheckedAt, c.IsUp, c.ResponseTimeMs, c.StatusCode, c.ErrorMessage);
}
