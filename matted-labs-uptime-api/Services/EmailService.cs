using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using MattedLabsUptime.Api.Models;
using MattedLabsUptime.Api.Repositories;

namespace MattedLabsUptime.Api.Services;

public interface IEmailService
{
    Task SendDownAlertAsync(string serviceName, string url, string? error);
    Task SendRecoveryAlertAsync(string serviceName, string url, long responseTimeMs);
    Task<(bool Sent, string? Error)> SendTestEmailAsync(AppSettings? overrideSettings = null);
}

public class EmailService(ISettingsRepository settingsRepo, ILogger<EmailService> logger) : IEmailService
{
    public async Task SendDownAlertAsync(string serviceName, string url, string? error) =>
        await SendAsync(
            settings: null,
            subject: $"[DOWN] {serviceName}",
            body: $"Service is DOWN.\n\nName: {serviceName}\nURL: {url}\nTime: {DateTime.UtcNow:u}\nError: {error ?? "Unknown"}"
        );

    public async Task SendRecoveryAlertAsync(string serviceName, string url, long responseTimeMs) =>
        await SendAsync(
            settings: null,
            subject: $"[UP] {serviceName} recovered",
            body: $"Service has recovered.\n\nName: {serviceName}\nURL: {url}\nTime: {DateTime.UtcNow:u}\nResponse: {responseTimeMs}ms"
        );

    public async Task<(bool Sent, string? Error)> SendTestEmailAsync(AppSettings? overrideSettings = null) =>
        await SendAsync(
            settings: overrideSettings,
            subject: "[Matted Labs Uptime] Test email",
            body: $"Your alert configuration is working.\n\nSent at: {DateTime.UtcNow:u}"
        );

    private async Task<(bool Sent, string? Error)> SendAsync(AppSettings? settings, string subject, string body)
    {
        var s = settings ?? await settingsRepo.GetAsync();

        if (!s.AlertsEnabled)
        {
            logger.LogInformation("Alerts disabled, skipping email: {Subject}", subject);
            return (false, "Alerts are disabled");
        }

        if (string.IsNullOrWhiteSpace(s.SmtpHost))
        {
            logger.LogInformation("SMTP not configured, skipping email: {Subject}", subject);
            return (false, "SMTP host is not configured");
        }

        try
        {
            var message = new MimeMessage();
            message.From.Add(MailboxAddress.Parse(s.SmtpFrom));
            message.To.Add(MailboxAddress.Parse(s.AlertRecipient));
            message.Subject = subject;
            message.Body = new TextPart("plain") { Text = body };

            using var client = new SmtpClient();
            await client.ConnectAsync(s.SmtpHost, s.SmtpPort, SecureSocketOptions.Auto);
            await client.AuthenticateAsync(s.SmtpUser, s.SmtpPassword);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);

            logger.LogInformation("Email sent: {Subject}", subject);
            return (true, null);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send email: {Subject}", subject);
            return (false, ex.Message);
        }
    }
}
