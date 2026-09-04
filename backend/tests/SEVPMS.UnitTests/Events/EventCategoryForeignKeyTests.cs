using SEVPMS.Application.Features.Events.DTOs;
using SEVPMS.Application.Features.Events.Services;
using SEVPMS.Application.Interfaces.Repositories;
using SEVPMS.Domain.Entities.Events;
using SEVPMS.Domain.Entities.VenueRentals;
using SEVPMS.Domain.Entities.Venues;
using Xunit;

namespace SEVPMS.UnitTests.Events;

public sealed class EventCategoryForeignKeyTests
{
    [Fact]
    public async Task Create_with_CategoryId_stores_category_foreign_key()
    {
        var organizerId = Guid.NewGuid();

        var venue = NewVenue();

        var category = new EventCategory
        {
            Id = Guid.NewGuid(),
            Name = "Concert",
            Code = "CONCERT",
            IsActive = true
        };

        var eventRepository =
            new FakeEventRepository();

        var service =
            new EventService(
                eventRepository,
                new FakeVenueRepository(venue),
                new FakeVenueRentalRepository(),
                new FakeEventCategoryRepository(category));

        var request =
            new CreateEventRequest
            {
                VenueId = venue.Id,

                CategoryId = category.Id,

                Title = "Music Night",

                Description = "Test concert",

                StartAtUtc =
                    DateTime.UtcNow.AddDays(2),

                EndAtUtc =
                    DateTime.UtcNow
                        .AddDays(2)
                        .AddHours(3)
            };

        var result =
            await service.CreateAsync(
                organizerId,
                request);

        Assert.NotNull(
            eventRepository.AddedEvent);

        Assert.Equal(
            category.Id,
            eventRepository.AddedEvent!.CategoryId);

        Assert.Equal(
            category.Id,
            result.CategoryId);

        Assert.Equal(
            "Concert",
            result.Category);

        Assert.Equal(
            "Concert",
            eventRepository.AddedEvent.Category);
    }

    [Fact]
    public async Task Legacy_category_name_resolves_to_CategoryId()
    {
        var organizerId = Guid.NewGuid();

        var venue = NewVenue();

        var category = new EventCategory
        {
            Id = Guid.NewGuid(),
            Name = "Conference",
            Code = "CONF",
            IsActive = true
        };

        var eventRepository =
            new FakeEventRepository();

        var service =
            new EventService(
                eventRepository,
                new FakeVenueRepository(venue),
                new FakeVenueRentalRepository(),
                new FakeEventCategoryRepository(category));

        var request =
            new CreateEventRequest
            {
                VenueId = venue.Id,

                // Old frontend compatibility:
                CategoryId = Guid.Empty,
                Category = "Conference",

                Title = "Tech Conference",

                Description = "Compatibility test",

                StartAtUtc =
                    DateTime.UtcNow.AddDays(3),

                EndAtUtc =
                    DateTime.UtcNow
                        .AddDays(3)
                        .AddHours(5)
            };

        var result =
            await service.CreateAsync(
                organizerId,
                request);

        Assert.Equal(
            category.Id,
            result.CategoryId);

        Assert.Equal(
            category.Id,
            eventRepository.AddedEvent!.CategoryId);

        Assert.Equal(
            "Conference",
            result.Category);
    }

    [Fact]
    public async Task Inactive_CategoryId_is_rejected()
    {
        var venue = NewVenue();

        var category = new EventCategory
        {
            Id = Guid.NewGuid(),
            Name = "Inactive Category",
            Code = "INACTIVE",
            IsActive = false
        };

        var service =
            new EventService(
                new FakeEventRepository(),
                new FakeVenueRepository(venue),
                new FakeVenueRentalRepository(),
                new FakeEventCategoryRepository(category));

        var request =
            new CreateEventRequest
            {
                VenueId = venue.Id,

                CategoryId = category.Id,

                Title = "Invalid Event",

                StartAtUtc =
                    DateTime.UtcNow.AddDays(2),

                EndAtUtc =
                    DateTime.UtcNow
                        .AddDays(2)
                        .AddHours(2)
            };

        var exception =
            await Assert.ThrowsAsync<ArgumentException>(
                () => service.CreateAsync(
                    Guid.NewGuid(),
                    request));

        Assert.Contains(
            "active category",
            exception.Message,
            StringComparison.OrdinalIgnoreCase);
    }

    private static Venue NewVenue()
    {
        return new Venue
        {
            Id = Guid.NewGuid(),

            OwnerUserId = Guid.NewGuid(),

            Name = "Test Venue",

            Description = "Venue",

            AddressLine1 = "123 Test Road",

            City = "Colombo",

            District = "Colombo",

            Country = "Sri Lanka",

            Capacity = 500,

            IsActive = true
        };
    }

    private sealed class FakeEventRepository
        : IEventRepository
    {
        public Event? AddedEvent
        {
            get;
            private set;
        }

        public Task<IReadOnlyList<Event>>
            GetPublishedAsync(
                EventSearchRequest request,
                CancellationToken cancellationToken = default)
            => Task.FromResult<
                IReadOnlyList<Event>>(
                    Array.Empty<Event>());

        public Task<IReadOnlyList<Event>>
            GetByOrganizerUserIdAsync(
                Guid organizerUserId,
                CancellationToken cancellationToken = default)
            => Task.FromResult<
                IReadOnlyList<Event>>(
                    Array.Empty<Event>());

        public Task<Event?> GetByIdAsync(
            Guid eventId,
            CancellationToken cancellationToken = default)
            => Task.FromResult<Event?>(null);

        public Task AddAsync(
            Event eventEntity,
            CancellationToken cancellationToken = default)
        {
            AddedEvent = eventEntity;

            return Task.CompletedTask;
        }

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
                    venue.OwnerUserId == ownerUserId
                        ? new[] { venue }
                        : Array.Empty<Venue>());

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

    private sealed class FakeEventCategoryRepository(
        EventCategory category)
        : IEventCategoryRepository
    {
        public Task<IReadOnlyList<EventCategory>>
            GetAsync(
                bool includeInactive,
                CancellationToken cancellationToken = default)
            => Task.FromResult<
                IReadOnlyList<EventCategory>>(
                    new[] { category });

        public Task<EventCategory?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
            => Task.FromResult<EventCategory?>(
                category.Id == id
                    ? category
                    : null);

        public Task<EventCategory?> FindActiveAsync(
            string nameOrCode,
            CancellationToken cancellationToken = default)
        {
            var matches =
                category.IsActive &&
                (
                    string.Equals(
                        category.Name,
                        nameOrCode,
                        StringComparison.OrdinalIgnoreCase)
                    ||
                    string.Equals(
                        category.Code,
                        nameOrCode,
                        StringComparison.OrdinalIgnoreCase)
                );

            return Task.FromResult<EventCategory?>(
                matches
                    ? category
                    : null);
        }

        public Task AddAsync(
            EventCategory value,
            CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task SaveChangesAsync(
            CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }

    private sealed class FakeVenueRentalRepository
        : IVenueRentalRepository
    {
        public Task<VenueRentalRequest?> GetByIdAsync(
            Guid rentalId,
            CancellationToken cancellationToken = default)
            => Task.FromResult<
                VenueRentalRequest?>(null);

        public Task<IReadOnlyList<VenueRentalRequest>>
            GetByOrganizerAsync(
                Guid organizerUserId,
                CancellationToken cancellationToken = default)
            => Task.FromResult<
                IReadOnlyList<VenueRentalRequest>>(
                    Array.Empty<VenueRentalRequest>());

        public Task<IReadOnlyList<VenueRentalRequest>>
            GetByVenueIdsAsync(
                IReadOnlyCollection<Guid> venueIds,
                CancellationToken cancellationToken = default)
            => Task.FromResult<
                IReadOnlyList<VenueRentalRequest>>(
                    Array.Empty<VenueRentalRequest>());

        public Task<bool> HasAcceptedOverlapAsync(
            Guid venueId,
            DateTime startAtUtc,
            DateTime endAtUtc,
            Guid? excludeRentalId = null,
            CancellationToken cancellationToken = default)
            => Task.FromResult(false);

        public Task<bool>
            HasAcceptedRentalForOrganizerAsync(
                Guid organizerUserId,
                Guid venueId,
                DateTime startAtUtc,
                DateTime endAtUtc,
                CancellationToken cancellationToken = default)
            => Task.FromResult(true);

        public Task AddAsync(
            VenueRentalRequest request,
            CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task SaveChangesAsync(
            CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }
}