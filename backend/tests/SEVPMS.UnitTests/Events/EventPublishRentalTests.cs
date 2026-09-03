using Xunit;
using SEVPMS.Application.Features.Events.DTOs;
using SEVPMS.Application.Features.Events.Services;
using SEVPMS.Application.Interfaces.Repositories;
using SEVPMS.Domain.Entities.Events;
using SEVPMS.Domain.Entities.VenueRentals;
using SEVPMS.Domain.Entities.Venues;
using SEVPMS.Domain.Enums;

namespace SEVPMS.UnitTests.Events;

public sealed class EventPublishRentalTests
{
    [Fact]
    public async Task Publish_requires_accepted_rental_covering_event_time()
    {
        var organizerId = Guid.NewGuid();
        var venueId = Guid.NewGuid();

        var eventEntity = NewDraftEvent(
            organizerId,
            venueId);

        var events = new FakeEventRepository(eventEntity);
        var venues = new FakeVenueRepository(
            new Venue
            {
                Id = venueId,
                OwnerUserId = Guid.NewGuid(),
                Name = "Venue",
                Description = "Venue",
                AddressLine1 = "Address",
                City = "Colombo",
                District = "Colombo",
                Capacity = 100,
                IsActive = true
            });

        var rentals = new FakeVenueRentalRepository
        {
            HasAcceptedRental = false
        };

        var service =
            new EventService(
                events,
                venues,
                rentals);

        var exception =
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.PublishAsync(
                    organizerId,
                    eventEntity.Id));

        Assert.Contains(
            "accepted venue rental",
            exception.Message,
            StringComparison.OrdinalIgnoreCase);

        Assert.Equal(
            EventStatus.Draft,
            eventEntity.Status);
    }

    [Fact]
    public async Task Publish_succeeds_when_accepted_rental_covers_event_time()
    {
        var organizerId = Guid.NewGuid();
        var venueId = Guid.NewGuid();

        var eventEntity = NewDraftEvent(
            organizerId,
            venueId);

        var events = new FakeEventRepository(eventEntity);
        var venues = new FakeVenueRepository(
            new Venue
            {
                Id = venueId,
                OwnerUserId = Guid.NewGuid(),
                Name = "Venue",
                Description = "Venue",
                AddressLine1 = "Address",
                City = "Colombo",
                District = "Colombo",
                Capacity = 100,
                IsActive = true
            });

        var rentals = new FakeVenueRentalRepository
        {
            HasAcceptedRental = true
        };

        var service =
            new EventService(
                events,
                venues,
                rentals);

        var result =
            await service.PublishAsync(
                organizerId,
                eventEntity.Id);

        Assert.Equal(
            EventStatus.Published,
            result.Status);

        Assert.Equal(
            EventStatus.Published,
            eventEntity.Status);
    }

    private static Event NewDraftEvent(
        Guid organizerId,
        Guid venueId)
        => new()
        {
            Id = Guid.NewGuid(),
            OrganizerUserId = organizerId,
            VenueId = venueId,
            Title = "Concert",
            Description = "Test",
            Category = "Music",
            StartAtUtc = DateTime.UtcNow.AddDays(2),
            EndAtUtc = DateTime.UtcNow.AddDays(2).AddHours(3),
            Status = EventStatus.Draft
        };

    private sealed class FakeEventRepository(
        Event eventEntity)
        : IEventRepository
    {
        public Task<IReadOnlyList<Event>> GetPublishedAsync(
            EventSearchRequest request,
            CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<Event>>(
                Array.Empty<Event>());

        public Task<IReadOnlyList<Event>> GetByOrganizerUserIdAsync(
            Guid organizerUserId,
            CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<Event>>(
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
        public Task<IReadOnlyList<Venue>> GetAllAsync(
            CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<Venue>>(
                new[] { venue });

        public Task<IReadOnlyList<Venue>> GetByOwnerUserIdAsync(
            Guid ownerUserId,
            CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<Venue>>(
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

    private sealed class FakeVenueRentalRepository
        : IVenueRentalRepository
    {
        public bool HasAcceptedRental { get; set; }

        public Task<VenueRentalRequest?> GetByIdAsync(
            Guid rentalId,
            CancellationToken cancellationToken = default)
            => Task.FromResult<VenueRentalRequest?>(null);

        public Task<IReadOnlyList<VenueRentalRequest>> GetByOrganizerAsync(
            Guid organizerUserId,
            CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<VenueRentalRequest>>(
                Array.Empty<VenueRentalRequest>());

        public Task<IReadOnlyList<VenueRentalRequest>> GetByVenueIdsAsync(
            IReadOnlyCollection<Guid> venueIds,
            CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<VenueRentalRequest>>(
                Array.Empty<VenueRentalRequest>());

        public Task<bool> HasAcceptedOverlapAsync(
            Guid venueId,
            DateTime startAtUtc,
            DateTime endAtUtc,
            Guid? excludeRentalId = null,
            CancellationToken cancellationToken = default)
            => Task.FromResult(false);

        public Task<bool> HasAcceptedRentalForOrganizerAsync(
            Guid organizerUserId,
            Guid venueId,
            DateTime startAtUtc,
            DateTime endAtUtc,
            CancellationToken cancellationToken = default)
            => Task.FromResult(HasAcceptedRental);

        public Task AddAsync(
            VenueRentalRequest request,
            CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task SaveChangesAsync(
            CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }
}
