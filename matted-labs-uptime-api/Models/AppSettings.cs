namespace MattedLabsUptime.Api.Models;

public class AppSettings
{
    public int Id { get; set; }
    public string SmtpHost { get; set; } = string.Empty;
    public int SmtpPort { get; set; } = 587;
    public string SmtpUser { get; set; } = string.Empty;
    public string SmtpPassword { get; set; } = string.Empty;
    public string SmtpFrom { get; set; } = string.Empty;
    public string AlertRecipient { get; set; } = string.Empty;
    public bool SmtpEnableSsl { get; set; } = true;
    public bool AlertsEnabled { get; set; } = true;
}
