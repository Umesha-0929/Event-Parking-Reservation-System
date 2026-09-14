namespace SEVPMS.Application.Features.Weather.DTOs;

public sealed class EventWeatherResponse
{
    public Guid EventId { get; set; }

    public string EventTitle { get; set; } = string.Empty;

    public Guid VenueId { get; set; }

    public string VenueName { get; set; } = string.Empty;

    public DateTime EventStartUtc { get; set; }

    public string Location { get; set; } = string.Empty;

    public bool Available { get; set; }

    public string? Message { get; set; }

    public int? WeatherCode { get; set; }

    public string? Condition { get; set; }

    public decimal? MinimumTemperatureC { get; set; }

    public decimal? MaximumTemperatureC { get; set; }

    public int? PrecipitationProbabilityPercent { get; set; }

    public decimal? PrecipitationMm { get; set; }

    public decimal? MaximumWindSpeedKmh { get; set; }

    public string? Warning { get; set; }
}