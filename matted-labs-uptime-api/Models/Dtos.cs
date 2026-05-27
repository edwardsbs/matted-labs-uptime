namespace MattedLabsUptime.Api.Models;

public record MonitoredServiceDto(
    int Id,
    string Name,
    string Url,
    bool IsActive,
    bool IgnoreSslErrors,
    int IntervalMinutes,
    DateTime CreatedAt
);

public record CreateServiceRequest(
    string Name,
    string Url,
    bool IsActive,
    bool IgnoreSslErrors,
    int IntervalMinutes
);

public record UpdateServiceRequest(
    string Name,
    string Url,
    bool IsActive,
    bool IgnoreSslErrors,
    int IntervalMinutes
);

public record UptimeCheckDto(
    int Id,
    int MonitoredServiceId,
    DateTime CheckedAt,
    bool IsUp,
    long ResponseTimeMs,
    int? StatusCode,
    string? ErrorMessage
);

public record ServiceStatusDto(
    MonitoredServiceDto Service,
    UptimeCheckDto? LatestCheck,
    double Uptime24h,
    double Uptime7d,
    double Uptime30d,
    IEnumerable<UptimeCheckDto> RecentChecks
);

public record UptimeStatsDto(
    double Uptime24h,
    double Uptime7d,
    double Uptime30d
);

public record TestUrlRequest(string Url, bool IgnoreSslErrors);

public record TestUrlResult(bool IsUp, long ResponseTimeMs, int? StatusCode, string? ErrorMessage);

public record AppSettingsDto(
    string SmtpHost,
    int SmtpPort,
    string SmtpUser,
    string SmtpPassword,
    string SmtpFrom,
    string AlertRecipient,
    bool SmtpEnableSsl,
    bool AlertsEnabled
);

public record UpdateSettingsRequest(
    string SmtpHost,
    int SmtpPort,
    string SmtpUser,
    string SmtpPassword,
    string SmtpFrom,
    string AlertRecipient,
    bool SmtpEnableSsl,
    bool AlertsEnabled
);
