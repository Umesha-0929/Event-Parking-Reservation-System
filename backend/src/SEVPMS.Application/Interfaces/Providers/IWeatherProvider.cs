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

    Task<WeatherProviderForecast?> GetDailyForecastAsync(
        decimal latitude,
        decimal longitude,
        DateOnly date,
        CancellationToken cancellationToken = default)
    {
        // Backward-compatible default implementation.
        // Existing test/fake providers that only implement
        // location-based lookup will continue to compile.
        return Task.FromResult<WeatherProviderForecast?>(
            null);
    }
}