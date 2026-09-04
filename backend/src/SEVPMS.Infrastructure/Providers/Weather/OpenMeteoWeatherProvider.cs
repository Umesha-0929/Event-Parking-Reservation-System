using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using SEVPMS.Application.Interfaces.Providers;

namespace SEVPMS.Infrastructure.Providers.Weather;

public sealed class OpenMeteoWeatherProvider(
    HttpClient httpClient)
    : IWeatherProvider
{
    public async Task<WeatherProviderForecast?>
        GetDailyForecastAsync(
            string locationQuery,
            DateOnly date,
            CancellationToken cancellationToken = default)
    {
        try
        {
            var location =
                await GetLocationAsync(
                    locationQuery,
                    cancellationToken);

            if (location is null)
            {
                return null;
            }

            return await GetForecastAsync(
                location.Latitude,
                location.Longitude,
                date,
                cancellationToken);
        }
        catch (HttpRequestException)
        {
            return null;
        }
        catch (TaskCanceledException)
        {
            return null;
        }
    }

    public async Task<WeatherProviderForecast?>
        GetDailyForecastAsync(
            decimal latitude,
            decimal longitude,
            DateOnly date,
            CancellationToken cancellationToken = default)
    {
        if (latitude < -90m ||
            latitude > 90m ||
            longitude < -180m ||
            longitude > 180m)
        {
            return null;
        }

        try
        {
            return await GetForecastAsync(
                latitude,
                longitude,
                date,
                cancellationToken);
        }
        catch (HttpRequestException)
        {
            return null;
        }
        catch (TaskCanceledException)
        {
            return null;
        }
    }

    private async Task<WeatherProviderForecast?>
        GetForecastAsync(
            decimal latitudeValue,
            decimal longitudeValue,
            DateOnly date,
            CancellationToken cancellationToken)
    {
        var dateText =
            date.ToString(
                "yyyy-MM-dd",
                CultureInfo.InvariantCulture);

        var latitude =
            latitudeValue.ToString(
                CultureInfo.InvariantCulture);

        var longitude =
            longitudeValue.ToString(
                CultureInfo.InvariantCulture);

        var forecastUrl =
            "https://api.open-meteo.com/v1/forecast" +
            $"?latitude={latitude}" +
            $"&longitude={longitude}" +
            "&daily=weather_code," +
            "temperature_2m_max," +
            "temperature_2m_min," +
            "precipitation_sum," +
            "precipitation_probability_max," +
            "wind_speed_10m_max" +
            "&timezone=UTC" +
            $"&start_date={dateText}" +
            $"&end_date={dateText}";

        var response =
            await httpClient.GetFromJsonAsync<
                ForecastResponse>(
                    forecastUrl,
                    cancellationToken);

        var daily =
            response?.Daily;

        if (daily is null ||
            daily.WeatherCode.Count == 0 ||
            daily.TemperatureMax.Count == 0 ||
            daily.TemperatureMin.Count == 0)
        {
            return null;
        }

        return new WeatherProviderForecast(
            WeatherCode:
                daily.WeatherCode[0],

            MinimumTemperatureC:
                daily.TemperatureMin[0],

            MaximumTemperatureC:
                daily.TemperatureMax[0],

            PrecipitationProbabilityPercent:
                daily.PrecipitationProbabilityMax
                    .FirstOrDefault(),

            PrecipitationMm:
                daily.PrecipitationSum
                    .FirstOrDefault(),

            MaximumWindSpeedKmh:
                daily.MaximumWindSpeed
                    .FirstOrDefault());
    }

    private async Task<GeoResult?>
        GetLocationAsync(
            string locationQuery,
            CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(
            locationQuery))
        {
            return null;
        }

        var encoded =
            Uri.EscapeDataString(
                locationQuery);

        var url =
            "https://geocoding-api.open-meteo.com/v1/search" +
            $"?name={encoded}" +
            "&count=1" +
            "&language=en" +
            "&format=json";

        var response =
            await httpClient.GetFromJsonAsync<
                GeocodingResponse>(
                    url,
                    cancellationToken);

        return response?
            .Results?
            .FirstOrDefault();
    }

    private sealed class GeocodingResponse
    {
        [JsonPropertyName("results")]
        public List<GeoResult>? Results
        {
            get;
            set;
        }
    }

    private sealed class GeoResult
    {
        [JsonPropertyName("latitude")]
        public decimal Latitude
        {
            get;
            set;
        }

        [JsonPropertyName("longitude")]
        public decimal Longitude
        {
            get;
            set;
        }
    }

    private sealed class ForecastResponse
    {
        [JsonPropertyName("daily")]
        public DailyForecast? Daily
        {
            get;
            set;
        }
    }

    private sealed class DailyForecast
    {
        [JsonPropertyName("weather_code")]
        public List<int> WeatherCode
        {
            get;
            set;
        } = new();

        [JsonPropertyName("temperature_2m_max")]
        public List<decimal> TemperatureMax
        {
            get;
            set;
        } = new();

        [JsonPropertyName("temperature_2m_min")]
        public List<decimal> TemperatureMin
        {
            get;
            set;
        } = new();

        [JsonPropertyName(
            "precipitation_probability_max")]
        public List<int>
            PrecipitationProbabilityMax
        {
            get;
            set;
        } = new();

        [JsonPropertyName("precipitation_sum")]
        public List<decimal> PrecipitationSum
        {
            get;
            set;
        } = new();

        [JsonPropertyName("wind_speed_10m_max")]
        public List<decimal> MaximumWindSpeed
        {
            get;
            set;
        } = new();
    }
}