using Microsoft.AspNetCore.Mvc;
using MattedLabsUptime.Api.Models;
using MattedLabsUptime.Api.Repositories;
using MattedLabsUptime.Api.Services;

namespace MattedLabsUptime.Api.Controllers;

[ApiController]
[Route("api/settings")]
public class SettingsController(ISettingsRepository settingsRepo, IEmailService emailService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var s = await settingsRepo.GetAsync();
        return Ok(ToDto(s));
    }

    [HttpPut]
    public async Task<IActionResult> Update([FromBody] UpdateSettingsRequest req)
    {
        var s = await settingsRepo.GetAsync();
        s.SmtpHost       = req.SmtpHost;
        s.SmtpPort       = req.SmtpPort;
        s.SmtpUser       = req.SmtpUser;
        s.SmtpFrom       = req.SmtpFrom;
        s.AlertRecipient = req.AlertRecipient;
        s.SmtpEnableSsl  = req.SmtpEnableSsl;
        s.AlertsEnabled  = req.AlertsEnabled;
        // Only overwrite password if the caller sent one
        if (!string.IsNullOrEmpty(req.SmtpPassword))
            s.SmtpPassword = req.SmtpPassword;
        await settingsRepo.UpdateAsync(s);
        return Ok(ToDto(s));
    }

    [HttpPost("test-email")]
    public async Task<IActionResult> TestEmail()
    {
        var sent = await emailService.SendTestEmailAsync();
        return Ok(new { sent });
    }

    private static AppSettingsDto ToDto(AppSettings s) => new(
        s.SmtpHost, s.SmtpPort, s.SmtpUser, s.SmtpPassword,
        s.SmtpFrom, s.AlertRecipient, s.SmtpEnableSsl, s.AlertsEnabled
    );
}
