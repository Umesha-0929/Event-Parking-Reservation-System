using SEVPMS.Application.Features.Events.DTOs;
using SEVPMS.Application.Features.Weather.Services;
using SEVPMS.Application.Interfaces.Providers;
using SEVPMS.Application.Interfaces.Repositories;
using SEVPMS.Domain.Entities.Events;
using SEVPMS.Domain.Entities.Venues;
using SEVPMS.Domain.Enums;
using Xunit;

namespace SEVPMS.UnitTests.Weather;

public sealed class WeatherCoordinatePreferenceTests
{
    [Fact]
    public async Task Stored_coordinates_are_preferred_over_geocoding()
    {
        var venue = NewVenue(
            latitude: 6.927079m,
            longitude: 79.861244m);

        var eventEntity =
            NewPublishedEvent(venue.Id);

        var provider =
            new FakeWeatherProvider();

        var service =
            new WeatherService(
                new FakeEventRepository(eventEntity),
                new FakeVenueRepository(venue),
                provider);

        var result =
            await service.GetEventWeatherAsync(
                eventEntity.Id);

        Assert.True(result.Available);

        Assert.Equal(
            1,
            provider.CoordinateCalls);

        Assert.Equal(
            0,
            provider.LocationCalls);

        Assert.Equal(
            6.927079m,
            provider.LastLatitude);

        Assert.Equal(
            79.861244m,
            provider.LastLongitude);
    }

    [Fact]
    public async Task Missing_coordinates_fall_back_to_location_geocoding()
    {
        var venue = NewVenue(
            latitude: null,
            longitude: null);

        var eventEntity =
            NewPublishedEvent(venue.Id);

        var provider =
            new FakeWeatherProvider();

        var service =
            new WeatherService(
                new FakeEventRepository(eventEntity),
                new FakeVenueRepository(venue),
                provider);

        var result =
            await service.GetEventWeatherAsync(
                eventEntity.Id);

        Assert.True(result.Available);

        Assert.Equal(
            0,
            provider.CoordinateCalls);

        Assert.Equal(
            1,
            provider.LocationCalls);

        Assert.Contains(
            "Colombo",
            provider.LastLocation ?? string.Empty,
            StringComparison.OrdinalIgnoreCase);

        Assert.Contains(
            "Sri Lanka",
            provider.LastLocation ?? string.Empty,
            StringComparison.OrdinalIgnoreCase);
    }

    private static Event NewPublishedEvent(
        Guid venueId)
    {
        return new Event
        {
            Id = Guid.NewGuid(),
            OrganizerUserId = Guid.NewGuid(),
            VenueId = venueId,
            CategoryId = Guid.NewGuid(),
            Title = "Weather Test Event",
            Description = "Weather test",
            StartAtUtc =
                DateTime.UtcNow.AddDays(2),
            EndAtUtc =
                DateTime.UtcNow
                    .AddDays(2)
                    .AddHours(3),
            Status = EventStatus.Published
        };
    }

    private static Venue NewVenue(
        decimal? latitude,
        decimal? longitude)
    {
        return new Venue
        {
            Id = Guid.NewGuid(),
            OwnerUserId = Guid.NewGuid(),
            Name = "Colombo Test Venue",
            Description = "Test venue",
            AddressLine1 = "123 Test Road",
            City = "Colombo",
            District = "Colombo",
            Country = "Sri Lanka",
            Latitude = latitude,
            Longitude = longitude,
            Capacity = 500,
            IsActive = true
        };
    }

    private sealed class FakeWeatherProvider
        : IWeatherProvider
    {
        public int LocationCalls
        {
            get;
            private set;
        }

        public int CoordinateCalls
        {
            get;
            private set;
        }

        public string? LastLocation
        {
            get;
            private set;
        }

        public decimal? LastLatitude
        {
            get;
            private set;
        }

        public decimal? LastLongitude
        {
            get;
            private set;
        }

        public Task<WeatherProviderForecast?>
            GetDailyForecastAsync(
                string locationQuery,
                DateOnly date,
                CancellationToken cancellationToken = default)
        {
            LocationCalls++;
            LastLocation = locationQuery;

            return Task.FromResult<
                WeatherProviderForecast?>(
                    Forecast());
        }

        public Task<WeatherProviderForecast?>
            GetDailyForecastAsync(
                decimal latitude,
                decimal longitude,
                DateOnly date,
                CancellationToken cancellationToken = default)
        {
            CoordinateCalls++;
            LastLatitude = latitude;
            LastLongitude = longitude;

            return Task.FromResult<
                WeatherProviderForecast?>(
                    Forecast());
        }

        private static WeatherProviderForecast
            Forecast()
        {
            return new WeatherProviderForecast(
                WeatherCode: 1,
                MinimumTemperatureC: 25m,
                MaximumTemperatureC: 31m,
                PrecipitationProbabilityPercent: 20,
                PrecipitationMm: 0.5m,
                MaximumWindSpeedKmh: 15m);
        }
    }

    private sealed class FakeEventRepository(
        Event eventEntity)
        : IEventRepository
    {
        public Task<IReadOnlyList<Event>>
            GetPublishedAsync(
                EventSearchRequest request,
                CancellationToken cancellationToken = default)
            => Task.FromResult<
                IReadOnlyList<Event>>(
                    new[] { eventEntity });

        public Task<IReadOnlyList<Event>>
            GetByOrganizerUserIdAsync(
                Guid organizerUserId,
                CancellationToken cancellationToken = default)
            => Task.FromResult<
                IReadOnlyList<Event>>(
                    new[] { eventEntity });

        public Task<Event?> GetByIdAsync(
            Guid eventId,
            CancellationToken cancellationToken = default)
            => Task.FromResult<Event?>(
                eventEntity.Id == eventId
                    ? eventEntity
                    : null);

        public Task AddAsync(
            Event value,
            CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task SaveChangesAsync(
            CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }

    private sealed class FakeVenueRepository(
        Venue venue)
        : IVenueRepository
    {
        public Task<IReadOnlyList<Venue>>
            GetAllAsync(
                CancellationToken cancellationToken = default)
            => Task.FromResult<
                IReadOnlyList<Venue>>(
                    new[] { venue });

        public Task<IReadOnlyList<Venue>>
            GetByOwnerUserIdAsync(
                Guid ownerUserId,
                CancellationToken cancellationToken = default)
            => Task.FromResult<
                IReadOnlyList<Venue>>(
                    new[] { venue });

        public Task<Venue?> GetByIdAsync(
            Guid venueId,
            CancellationToken cancellationToken = default)
            => Task.FromResult<Venue?>(
                venue.Id == venueId
                    ? venue
                    : null);

        public Task AddAsync(
            Venue value,
            CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task SaveChangesAsync(
            CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }
}