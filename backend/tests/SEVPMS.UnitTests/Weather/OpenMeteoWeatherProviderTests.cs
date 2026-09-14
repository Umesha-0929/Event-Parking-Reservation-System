using System.Net;
using System.Text;
using SEVPMS.Infrastructure.Providers.Weather;
using Xunit;

namespace SEVPMS.UnitTests.Weather;

public sealed class OpenMeteoWeatherProviderTests
{
    [Fact]
    public async Task GetDailyForecastAsync_maps_open_meteo_response()
    {
        var handler =
            new FakeHttpMessageHandler();

        using var httpClient =
            new HttpClient(handler);

        var provider =
            new OpenMeteoWeatherProvider(
                httpClient);

        var result =
            await provider.GetDailyForecastAsync(
                "Colombo, Colombo, Sri Lanka",
                DateOnly.FromDateTime(
                    DateTime.UtcNow.AddDays(2)));

        Assert.NotNull(result);

        Assert.Equal(
            95,
            result!.WeatherCode);

        Assert.Equal(
            24.5m,
            result.MinimumTemperatureC);

        Assert.Equal(
            31.2m,
            result.MaximumTemperatureC);

        Assert.Equal(
            75,
            result.PrecipitationProbabilityPercent);

        Assert.Equal(
            8.4m,
            result.PrecipitationMm);

        Assert.Equal(
            36.5m,
            result.MaximumWindSpeedKmh);

        Assert.Equal(
            2,
            handler.RequestCount);
    }

    private sealed class FakeHttpMessageHandler
        : HttpMessageHandler
    {
        public int RequestCount
        {
            get;
            private set;
        }

        protected override Task<HttpResponseMessage>
            SendAsync(
                HttpRequestMessage request,
                CancellationToken cancellationToken)
        {
            RequestCount++;

            var url =
                request.RequestUri?.ToString()
                ?? string.Empty;

            if (url.Contains(
                "geocoding-api.open-meteo.com",
                StringComparison.OrdinalIgnoreCase))
            {
                return Task.FromResult(
                    JsonResponse(
                        """
                        {
                          "results": [
                            {
                              "latitude": 6.9271,
                              "longitude": 79.8612
                            }
                          ]
                        }
                        """));
            }

            if (url.Contains(
                "api.open-meteo.com",
                StringComparison.OrdinalIgnoreCase))
            {
                return Task.FromResult(
                    JsonResponse(
                        """
                        {
                          "daily": {
                            "weather_code": [95],
                            "temperature_2m_max": [31.2],
                            "temperature_2m_min": [24.5],
                            "precipitation_sum": [8.4],
                            "precipitation_probability_max": [75],
                            "wind_speed_10m_max": [36.5]
                          }
                        }
                        """));
            }

            return Task.FromResult(
                new HttpResponseMessage(
                    HttpStatusCode.NotFound));
        }

        private static HttpResponseMessage
            JsonResponse(
                string json)
        {
            return new HttpResponseMessage(
                HttpStatusCode.OK)
            {
                Content =
                    new StringContent(
                        json,
                        Encoding.UTF8,
                        "application/json")
            };
        }
    }
}