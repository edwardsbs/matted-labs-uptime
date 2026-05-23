using Microsoft.AspNetCore.Mvc;
using MattedLabsUptime.Api.Models;
using MattedLabsUptime.Api.Repositories;

namespace MattedLabsUptime.Api.Controllers;

[ApiController]
[Route("api/services")]
public class ServicesController(IServiceRepository repo) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var services = await repo.GetAllAsync();
        return Ok(services.Select(ToDto));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var service = await repo.GetByIdAsync(id);
        return service is null ? NotFound() : Ok(ToDto(service));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateServiceRequest request)
    {
        var service = new MonitoredService
        {
            Name = request.Name,
            Url = request.Url,
            IsActive = request.IsActive,
            IgnoreSslErrors = request.IgnoreSslErrors,
            IntervalMinutes = request.IntervalMinutes
        };
        var created = await repo.CreateAsync(service);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, ToDto(created));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateServiceRequest request)
    {
        var updated = await repo.UpdateAsync(id, new MonitoredService
        {
            Name = request.Name,
            Url = request.Url,
            IsActive = request.IsActive,
            IgnoreSslErrors = request.IgnoreSslErrors,
            IntervalMinutes = request.IntervalMinutes
        });
        return updated is null ? NotFound() : Ok(ToDto(updated));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await repo.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }

    private static MonitoredServiceDto ToDto(MonitoredService s) =>
        new(s.Id, s.Name, s.Url, s.IsActive, s.IgnoreSslErrors, s.IntervalMinutes, s.CreatedAt);
}
