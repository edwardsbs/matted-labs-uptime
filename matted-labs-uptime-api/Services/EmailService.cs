using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using MattedLabsUptime.Api.Repositories;

namespace MattedLabsUptime.Api.Services;

public interface IEmailService
{
    Task SendDownAlertAsync(string serviceName, string url, string? error);
    Task SendRecoveryAlertAsync(string serviceName, string url, long responseTimeMs);
    Task<bool> SendTestEmailAsync();
}

public class EmailService(ISettingsRepository settingsRepo, ILogger<EmailService> logger) : IEmailService
{
    public async Task SendDownAlertAsync(string serviceName, string url, string? error) =>
        await SendAsync(
            subject: $"[DOWN] {serviceName}",
            body: $"Service is DOWN.\n\nName: {serviceName}\nURL: {url}\nTime: {DateTime.UtcNow:u}\nError: {error ?? "Unknown"}"
        );

    public async Task SendRecoveryAlertAsync(string serviceName, string url, long responseTimeMs) =>
        await SendAsync(
            subject: $"[UP] {serviceName} recovered",
            body: $"Service has recovered.\n\nName: {serviceName}\nURL: {url}\nTime: {DateTime.UtcNow:u}\nResponse: {responseTimeMs}ms"
        );

    public async Task<bool> SendTestEmailAsync() =>
        await SendAsync(
            subject: "[Matted Labs Uptime] Test email",
            body: $"Your alert configuration is working.\n\nSent at: {DateTime.UtcNow:u}"
        );

    private async Task<bool> SendAsync(string subject, string body)
    {
        var s = await settingsRepo.GetAsync();

        if (!s.AlertsEnabled)
        {
            logger.LogInformation("Alerts disabled, skipping email: {Subject}", subject);
            return false;
        }

        if (string.IsNullOrWhiteSpace(s.SmtpHost))
        {
            logger.LogInformation("SMTP not configured, skipping email: {Subject}", subject);
            return false;
        }

        try
        {
            var message = new MimeMessage();
            message.From.Add(MailboxAddress.Parse(s.SmtpFrom));
            message.To.Add(MailboxAddress.Parse(s.AlertRecipient));
            message.Subject = subject;
            message.Body = new TextPart("plain") { Text = body };

            using var client = new SmtpClient();
            await client.ConnectAsync(s.SmtpHost, s.SmtpPort,
                s.SmtpEnableSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.None);
            await client.AuthenticateAsync(s.SmtpUser, s.SmtpPassword);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);

            logger.LogInformation("Email sent: {Subject}", subject);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send email: {Subject}", subject);
            return false;
        }
    }
}
