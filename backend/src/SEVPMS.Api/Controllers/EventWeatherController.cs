using Microsoft.AspNetCore.Mvc;
using SEVPMS.Application.Features.Weather.DTOs;
using SEVPMS.Application.Features.Weather.Interfaces;

namespace SEVPMS.Api.Controllers;

[ApiController]
[Route("api/events")]
public sealed class EventWeatherController(
    IWeatherService weatherService)
    : ControllerBase
{
    [HttpGet("{eventId:guid}/weather")]
    public async Task<
        ActionResult<EventWeatherResponse>>
        GetWeather(
            Guid eventId,
            CancellationToken cancellationToken)
    {
        var weather =
            await weatherService.GetEventWeatherAsync(
                eventId,
                cancellationToken);

        return Ok(weather);
    }
}