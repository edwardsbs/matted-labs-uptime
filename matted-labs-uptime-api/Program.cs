using Microsoft.EntityFrameworkCore;
using MattedLabsUptime.Api.BackgroundServices;
using MattedLabsUptime.Api.Data;
using MattedLabsUptime.Api.Repositories;
using MattedLabsUptime.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("Smtp"));

builder.Services.AddHttpClient("Default", c => c.Timeout = TimeSpan.FromSeconds(10));
builder.Services.AddHttpClient("IgnoreSsl", c => c.Timeout = TimeSpan.FromSeconds(10))
    .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
    {
        ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
    });

builder.Services.AddScoped<IServiceRepository, ServiceRepository>();
builder.Services.AddScoped<IUptimeCheckRepository, UptimeCheckRepository>();
builder.Services.AddScoped<IUptimeCheckerService, UptimeCheckerService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddHostedService<MonitoringBackgroundService>();

builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
    p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await DbSeeder.SeedAsync(db);
}

app.UseCors();
app.MapControllers();
app.MapGet("/api/health", () => new { ok = true, at = DateTime.UtcNow });

app.Run();
