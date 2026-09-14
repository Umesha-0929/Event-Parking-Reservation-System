using SEVPMS.Application.Features.Weather.DTOs;
using SEVPMS.Application.Features.Weather.Interfaces;
using SEVPMS.Application.Interfaces.Providers;
using SEVPMS.Application.Interfaces.Repositories;
using SEVPMS.Domain.Enums;

namespace SEVPMS.Application.Features.Weather.Services;

public sealed class WeatherService(
    IEventRepository eventRepository,
    IVenueRepository venueRepository,
    IWeatherProvider weatherProvider)
    : IWeatherService
{
    public async Task<EventWeatherResponse> GetEventWeatherAsync(
        Guid eventId,
        CancellationToken cancellationToken = default)
    {
        var eventEntity =
            await eventRepository.GetByIdAsync(
                eventId,
                cancellationToken)
            ?? throw new KeyNotFoundException(
                "Event was not found.");

        if (eventEntity.Status != EventStatus.Published)
        {
            throw new KeyNotFoundException(
                "Published event was not found.");
        }

        var venue =
            await venueRepository.GetByIdAsync(
                eventEntity.VenueId,
                cancellationToken)
            ?? throw new KeyNotFoundException(
                "Venue was not found.");

        var eventDate =
            DateOnly.FromDateTime(
                eventEntity.StartAtUtc);

        var today =
            DateOnly.FromDateTime(
                DateTime.UtcNow);

        var location =
            BuildLocation(
                venue.City,
                venue.District,
                venue.Country);

        var response =
            new EventWeatherResponse
            {
                EventId = eventEntity.Id,
                EventTitle = eventEntity.Title,
                VenueId = venue.Id,
                VenueName = venue.Name,
                EventStartUtc =
                    eventEntity.StartAtUtc,
                Location = location
            };

        if (eventDate < today)
        {
            response.Available = false;
            response.Message =
                "Weather forecast is not available because the event date has already passed.";

            return response;
        }

        if (eventDate > today.AddDays(15))
        {
            response.Available = false;
            response.Message =
                "Weather forecast is not available yet. Try again within 16 days of the event.";

            return response;
        }

        WeatherProviderForecast? forecast;

        // Prefer precise stored venue coordinates.
        if (venue.Latitude.HasValue &&
            venue.Longitude.HasValue)
        {
            forecast =
                await weatherProvider
                    .GetDailyForecastAsync(
                        venue.Latitude.Value,
                        venue.Longitude.Value,
                        eventDate,
                        cancellationToken);
        }
        else
        {
            // Backward-compatible fallback for venues
            // created before coordinates were stored.
            forecast =
                await weatherProvider
                    .GetDailyForecastAsync(
                        location,
                        eventDate,
                        cancellationToken);
        }

        if (forecast is null)
        {
            response.Available = false;
            response.Message =
                "Weather information is temporarily unavailable.";

            return response;
        }

        response.Available = true;

        response.WeatherCode =
            forecast.WeatherCode;

        response.Condition =
            GetCondition(
                forecast.WeatherCode);

        response.MinimumTemperatureC =
            forecast.MinimumTemperatureC;

        response.MaximumTemperatureC =
            forecast.MaximumTemperatureC;

        response.PrecipitationProbabilityPercent =
            forecast.PrecipitationProbabilityPercent;

        response.PrecipitationMm =
            forecast.PrecipitationMm;

        response.MaximumWindSpeedKmh =
            forecast.MaximumWindSpeedKmh;

        response.Warning =
            BuildWarning(
                forecast);

        return response;
    }

    private static string BuildLocation(
        string city,
        string district,
        string country)
    {
        return string.Join(
            ", ",
            new[]
            {
                city,
                district,
                country
            }
            .Where(x =>
                !string.IsNullOrWhiteSpace(x))
            .Select(x =>
                x.Trim()));
    }

    private static string GetCondition(
        int weatherCode)
    {
        return weatherCode switch
        {
            0 =>
                "Clear sky",

            1 or 2 =>
                "Mainly clear or partly cloudy",

            3 =>
                "Overcast",

            45 or 48 =>
                "Fog",

            51 or 53 or 55 =>
                "Drizzle",

            56 or 57 =>
                "Freezing drizzle",

            61 or 63 or 65 =>
                "Rain",

            66 or 67 =>
                "Freezing rain",

            71 or 73 or 75 or 77 =>
                "Snow",

            80 or 81 or 82 =>
                "Rain showers",

            85 or 86 =>
                "Snow showers",

            95 or 96 or 99 =>
                "Thunderstorm",

            _ =>
                "Unknown weather condition"
        };
    }

    private static string? BuildWarning(
        WeatherProviderForecast forecast)
    {
        var warnings =
            new List<string>();

        if (forecast.WeatherCode
            is 95 or 96 or 99)
        {
            warnings.Add(
                "Thunderstorm conditions may affect the event.");
        }

        if (forecast.PrecipitationProbabilityPercent >= 70 ||
            forecast.PrecipitationMm >= 10m)
        {
            warnings.Add(
                "High chance of significant rainfall.");
        }

        if (forecast.MaximumWindSpeedKmh >= 40m)
        {
            warnings.Add(
                "Strong winds may affect outdoor event arrangements.");
        }

        if (forecast.MaximumTemperatureC >= 35m)
        {
            warnings.Add(
                "High temperature expected during the event day.");
        }

        return warnings.Count == 0
            ? null
            : string.Join(
                " ",
                warnings);
    }
}