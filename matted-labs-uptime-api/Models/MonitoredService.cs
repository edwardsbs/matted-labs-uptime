namespace MattedLabsUptime.Api.Models;

public class MonitoredService
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public bool IgnoreSslErrors { get; set; } = false;
    public int IntervalMinutes { get; set; } = 5;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<UptimeCheck> UptimeChecks { get; set; } = new List<UptimeCheck>();
}
