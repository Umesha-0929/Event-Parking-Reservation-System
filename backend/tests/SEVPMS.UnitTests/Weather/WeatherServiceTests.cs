using SEVPMS.Application.Features.Events.DTOs;
using SEVPMS.Application.Features.Weather.Services;
using SEVPMS.Application.Interfaces.Providers;
using SEVPMS.Application.Interfaces.Repositories;
using SEVPMS.Domain.Entities.Events;
using SEVPMS.Domain.Entities.Venues;
using SEVPMS.Domain.Enums;
using Xunit;

namespace SEVPMS.UnitTests.Weather;

public sealed class WeatherServiceTests
{
    [Fact]
    public async Task GetEventWeatherAsync_returns_forecast_for_published_event()
    {
        var venue = new Venue
        {
            Id = Guid.NewGuid(),
            OwnerUserId = Guid.NewGuid(),
            Name = "Colombo Test Venue",
            Description = "Test venue",
            AddressLine1 = "01 Test Road",
            City = "Colombo",
            District = "Colombo",
            Country = "Sri Lanka",
            Capacity = 1000,
            IsActive = true
        };

        var eventEntity = new Event
        {
            Id = Guid.NewGuid(),
            OrganizerUserId = Guid.NewGuid(),
            VenueId = venue.Id,
            Title = "Weather Test Event",
            Description = "Test event",
            Category = "Concert",
            StartAtUtc = DateTime.UtcNow.AddDays(3),
            EndAtUtc = DateTime.UtcNow.AddDays(3).AddHours(3),
            Status = EventStatus.Published
        };

        var provider = new FakeWeatherProvider(
            new WeatherProviderForecast(
                WeatherCode: 95,
                MinimumTemperatureC: 25m,
                MaximumTemperatureC: 31m,
                PrecipitationProbabilityPercent: 80,
                PrecipitationMm: 12m,
                MaximumWindSpeedKmh: 45m));

        var service = new WeatherService(
            new FakeEventRepository(eventEntity),
            new FakeVenueRepository(venue),
            provider);

        var result =
            await service.GetEventWeatherAsync(
                eventEntity.Id);

        Assert.True(result.Available);

        Assert.Equal(
            eventEntity.Id,
            result.EventId);

        Assert.Equal(
            venue.Id,
            result.VenueId);

        Assert.Equal(
            "Colombo, Colombo, Sri Lanka",
            result.Location);

        Assert.Equal(
            "Thunderstorm",
            result.Condition);

        Assert.Equal(
            25m,
            result.MinimumTemperatureC);

        Assert.Equal(
            31m,
            result.MaximumTemperatureC);

        Assert.Equal(
            80,
            result.PrecipitationProbabilityPercent);

        Assert.NotNull(result.Warning);

        Assert.Contains(
            "Thunderstorm",
            result.Warning!);

        Assert.True(
            provider.WasCalled);
    }

    [Fact]
    public async Task GetEventWeatherAsync_does_not_call_provider_when_event_is_too_far_away()
    {
        var venue = new Venue
        {
            Id = Guid.NewGuid(),
            OwnerUserId = Guid.NewGuid(),
            Name = "Future Venue",
            Description = "Test venue",
            AddressLine1 = "01 Future Road",
            City = "Kandy",
            District = "Kandy",
            Country = "Sri Lanka",
            Capacity = 500,
            IsActive = true
        };

        var eventEntity = new Event
        {
            Id = Guid.NewGuid(),
            OrganizerUserId = Guid.NewGuid(),
            VenueId = venue.Id,
            Title = "Future Event",
            Description = "Future event",
            Category = "Conference",
            StartAtUtc = DateTime.UtcNow.AddDays(30),
            EndAtUtc = DateTime.UtcNow.AddDays(30).AddHours(2),
            Status = EventStatus.Published
        };

        var provider =
            new FakeWeatherProvider(null);

        var service = new WeatherService(
            new FakeEventRepository(eventEntity),
            new FakeVenueRepository(venue),
            provider);

        var result =
            await service.GetEventWeatherAsync(
                eventEntity.Id);

        Assert.False(result.Available);

        Assert.False(
            provider.WasCalled);

        Assert.Contains(
            "within 16 days",
            result.Message!);
    }

    private sealed class FakeWeatherProvider
        : IWeatherProvider
    {
        private readonly WeatherProviderForecast? forecast;

        public bool WasCalled
        {
            get;
            private set;
        }

        public FakeWeatherProvider(
            WeatherProviderForecast? forecast)
        {
            this.forecast = forecast;
        }

        public Task<WeatherProviderForecast?>
            GetDailyForecastAsync(
                string locationQuery,
                DateOnly date,
                CancellationToken cancellationToken = default)
        {
            WasCalled = true;

            return Task.FromResult(
                forecast);
        }
    }

    private sealed class FakeEventRepository
        : IEventRepository
    {
        private readonly Event eventEntity;

        public FakeEventRepository(
            Event eventEntity)
        {
            this.eventEntity = eventEntity;
        }

        public Task<Event?> GetByIdAsync(
            Guid eventId,
            CancellationToken cancellationToken = default)
            => Task.FromResult<Event?>(
                eventEntity.Id == eventId
                    ? eventEntity
                    : null);

        public Task<IReadOnlyList<Event>>
            GetPublishedAsync(
                EventSearchRequest request,
                CancellationToken cancellationToken = default)
            => Task.FromResult<
                IReadOnlyList<Event>>(
                    new[]
                    {
                        eventEntity
                    });

        public Task<IReadOnlyList<Event>>
            GetByOrganizerUserIdAsync(
                Guid organizerUserId,
                CancellationToken cancellationToken = default)
            => Task.FromResult<
                IReadOnlyList<Event>>(
                    new[]
                    {
                        eventEntity
                    });

        public Task AddAsync(
            Event eventEntity,
            CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task SaveChangesAsync(
            CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }

    private sealed class FakeVenueRepository
        : IVenueRepository
    {
        private readonly Venue venue;

        public FakeVenueRepository(
            Venue venue)
        {
            this.venue = venue;
        }

        public Task<IReadOnlyList<Venue>> GetAllAsync(
            CancellationToken cancellationToken = default)
            => Task.FromResult<
                IReadOnlyList<Venue>>(
                    new[]
                    {
                        venue
                    });

        public Task<IReadOnlyList<Venue>>
            GetByOwnerUserIdAsync(
                Guid ownerUserId,
                CancellationToken cancellationToken = default)
            => Task.FromResult<
                IReadOnlyList<Venue>>(
                    new[]
                    {
                        venue
                    });

        public Task<Venue?> GetByIdAsync(
            Guid venueId,
            CancellationToken cancellationToken = default)
            => Task.FromResult<Venue?>(
                venue.Id == venueId
                    ? venue
                    : null);

        public Task AddAsync(
            Venue venue,
            CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task SaveChangesAsync(
            CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }
}