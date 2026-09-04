using SEVPMS.Application.Features.Weather.DTOs;

namespace SEVPMS.Application.Features.Weather.Interfaces;

public interface IWeatherService
{
    Task<EventWeatherResponse> GetEventWeatherAsync(
        Guid eventId,
        CancellationToken cancellationToken = default);
}