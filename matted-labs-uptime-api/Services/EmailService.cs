using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;

namespace MattedLabsUptime.Api.Services;

public class SmtpSettings
{
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 587;
    public string User { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string From { get; set; } = string.Empty;
    public string To { get; set; } = string.Empty;
}

public interface IEmailService
{
    Task SendDownAlertAsync(string serviceName, string url, string? error);
    Task SendRecoveryAlertAsync(string serviceName, string url, long responseTimeMs);
}

public class EmailService(IOptions<SmtpSettings> options, ILogger<EmailService> logger) : IEmailService
{
    private readonly SmtpSettings _smtp = options.Value;

    public async Task SendDownAlertAsync(string serviceName, string url, string? error)
    {
        await SendAsync(
            subject: $"[DOWN] {serviceName}",
            body: $"Service is DOWN.\n\nName: {serviceName}\nURL: {url}\nTime: {DateTime.UtcNow:u}\nError: {error ?? "Unknown"}"
        );
    }

    public async Task SendRecoveryAlertAsync(string serviceName, string url, long responseTimeMs)
    {
        await SendAsync(
            subject: $"[UP] {serviceName} recovered",
            body: $"Service has recovered.\n\nName: {serviceName}\nURL: {url}\nTime: {DateTime.UtcNow:u}\nResponse: {responseTimeMs}ms"
        );
    }

    private async Task SendAsync(string subject, string body)
    {
        if (string.IsNullOrWhiteSpace(_smtp.Host))
        {
            logger.LogInformation("SMTP not configured, skipping email: {Subject}", subject);
            return;
        }

        try
        {
            using var client = new SmtpClient(_smtp.Host, _smtp.Port)
            {
                Credentials = new NetworkCredential(_smtp.User, _smtp.Password),
                EnableSsl = true
            };
            var message = new MailMessage(_smtp.From, _smtp.To, subject, body);
            await client.SendMailAsync(message);
            logger.LogInformation("Email sent: {Subject}", subject);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send email: {Subject}", subject);
        }
    }
}
