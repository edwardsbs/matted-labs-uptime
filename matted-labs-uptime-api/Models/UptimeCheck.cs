namespace MattedLabsUptime.Api.Models;

public class UptimeCheck
{
    public int Id { get; set; }
    public int MonitoredServiceId { get; set; }
    public DateTime CheckedAt { get; set; } = DateTime.UtcNow;
    public bool IsUp { get; set; }
    public long ResponseTimeMs { get; set; }
    public int? StatusCode { get; set; }
    public string? ErrorMessage { get; set; }

    public MonitoredService MonitoredService { get; set; } = null!;
}
