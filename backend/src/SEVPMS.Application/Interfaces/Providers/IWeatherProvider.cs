namespace SEVPMS.Application.Interfaces.Providers;

public sealed record WeatherProviderForecast(
    int WeatherCode,
    decimal MinimumTemperatureC,
    decimal MaximumTemperatureC,
    int PrecipitationProbabilityPercent,
    decimal PrecipitationMm,
    decimal MaximumWindSpeedKmh);

public interface IWeatherProvider
{
    Task<WeatherProviderForecast?> GetDailyForecastAsync(
        string locationQuery,
        DateOnly date,
        CancellationToken cancellationToken = default);
}