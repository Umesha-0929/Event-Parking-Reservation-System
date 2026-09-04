using SEVPMS.Application.Features.Audit.DTOs;
using SEVPMS.Application.Features.Audit.Interfaces;
using SEVPMS.Application.Features.Events.DTOs;
using SEVPMS.Application.Features.Events.Services;
using SEVPMS.Application.Features.Notifications.DTOs;
using SEVPMS.Application.Features.Notifications.Interfaces;
using SEVPMS.Application.Features.VenueRentals.DTOs;
using SEVPMS.Application.Features.VenueRentals.Services;
using SEVPMS.Application.Interfaces.Repositories;
using SEVPMS.Domain.Entities.Events;
using SEVPMS.Domain.Entities.VenueRentals;
using SEVPMS.Domain.Entities.Venues;
using SEVPMS.Domain.Enums;
using Xunit;

namespace SEVPMS.UnitTests.Audit;

public sealed class EventAndVenueRentalAuditTests
{
    [Fact]
    public async Task Event_publish_writes_semantic_audit()
    {
        var organizerId = Guid.NewGuid();
        var venueId = Guid.NewGuid();

        var eventEntity = new Event
        {
            Id = Guid.NewGuid(),
            OrganizerUserId = organizerId,
            VenueId = venueId,
            CategoryId = Guid.NewGuid(),
            Category = "Music",
            Title = "Concert",
            Description = "Test event",
            StartAtUtc = DateTime.UtcNow.AddDays(2),
            EndAtUtc = DateTime.UtcNow.AddDays(2).AddHours(3),
            Status = EventStatus.Draft
        };

        var audit = new FakeAuditLogService();

        var service = new EventService(
            new FakeEventRepository(eventEntity),
            new FakeVenueRepository(
                new Venue
                {
                    Id = venueId,
                    OwnerUserId = Guid.NewGuid(),
                    Name = "Venue",
                    Description = "Test venue",
                    AddressLine1 = "Road",
                    City = "Colombo",
                    District = "Colombo",
                    Country = "Sri Lanka",
                    Capacity = 500,
                    IsActive = true
                }),
            new FakeVenueRentalRepository
            {
                HasAcceptedRental = true
            },
            null,
            audit);

        var result = await service.PublishAsync(
            organizerId,
            eventEntity.Id);

        Assert.Equal(
            EventStatus.Published,
            result.Status);

        var entry = Assert.Single(audit.Entries);

        Assert.Equal(
            organizerId,
            entry.ActorUserId);

        Assert.Equal(
            "Event published",
            entry.Action);

        Assert.Equal(
            "Event",
            entry.EntityType);

        Assert.Equal(
            eventEntity.Id.ToString(),
            entry.EntityId);

        Assert.Contains(
            "Draft",
            entry.BeforeSummary ?? string.Empty,
            StringComparison.OrdinalIgnoreCase);

        Assert.Contains(
            "Published",
            entry.AfterSummary ?? string.Empty,
            StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Event_cancel_writes_semantic_audit()
    {
        var organizerId = Guid.NewGuid();

        var eventEntity = new Event
        {
            Id = Guid.NewGuid(),
            OrganizerUserId = organizerId,
            VenueId = Guid.NewGuid(),
            CategoryId = Guid.NewGuid(),
            Category = "Music",
            Title = "Concert",
            Description = "Test event",
            StartAtUtc = DateTime.UtcNow.AddDays(2),
            EndAtUtc = DateTime.UtcNow.AddDays(2).AddHours(3),
            Status = EventStatus.Published
        };

        var audit = new FakeAuditLogService();

        var service = new EventService(
            new FakeEventRepository(eventEntity),
            new FakeVenueRepository(null),
            new FakeVenueRentalRepository(),
            null,
            audit);

        var result = await service.CancelAsync(
            organizerId,
            eventEntity.Id);

        Assert.Equal(
            EventStatus.Cancelled,
            result.Status);

        var entry = Assert.Single(audit.Entries);

        Assert.Equal(
            "Event cancelled",
            entry.Action);

        Assert.Contains(
            "Published",
            entry.BeforeSummary ?? string.Empty,
            StringComparison.OrdinalIgnoreCase);

        Assert.Contains(
            "Cancelled",
            entry.AfterSummary ?? string.Empty,
            StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Venue_rental_status_update_writes_semantic_audit()
    {
        var venueOwnerId = Guid.NewGuid();
        var organizerId = Guid.NewGuid();
        var venueId = Guid.NewGuid();

        var rental = new VenueRentalRequest
        {
            Id = Guid.NewGuid(),
            OrganizerUserId = organizerId,
            VenueId = venueId,
            StartAtUtc = DateTime.UtcNow.AddDays(2),
            EndAtUtc = DateTime.UtcNow.AddDays(3),
            Purpose = "Conference",
            OfferedAmount = 50000m,
            Status = RentalRequestStatus.Pending
        };

        var audit = new FakeAuditLogService();

        var service = new VenueRentalService(
            new FakeVenueRentalRepository(rental),
            new FakeVenueRepository(
                new Venue
                {
                    Id = venueId,
                    OwnerUserId = venueOwnerId,
                    Name = "Venue",
                    Description = "Test",
                    AddressLine1 = "Road",
                    City = "Colombo",
                    District = "Colombo",
                    Country = "Sri Lanka",
                    Capacity = 500,
                    IsActive = true
                }),
            new FakeNotificationService(),
            null,
            audit);

        var result = await service.UpdateStatusAsync(
            venueOwnerId,
            rental.Id,
            new UpdateVenueRentalStatusRequest
            {
                Status = RentalRequestStatus.Rejected,
                OwnerMessage = "Unavailable"
            });

        Assert.Equal(
            RentalRequestStatus.Rejected,
            result.Status);

        var entry = Assert.Single(audit.Entries);

        Assert.Equal(
            venueOwnerId,
            entry.ActorUserId);

        Assert.Equal(
            "Venue rental status updated",
            entry.Action);

        Assert.Equal(
            "VenueRental",
            entry.EntityType);

        Assert.Contains(
            "Pending",
            entry.BeforeSummary ?? string.Empty,
            StringComparison.OrdinalIgnoreCase);

        Assert.Contains(
            "Rejected",
            entry.AfterSummary ?? string.Empty,
            StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Venue_rental_cancel_writes_semantic_audit()
    {
        var organizerId = Guid.NewGuid();

        var rental = new VenueRentalRequest
        {
            Id = Guid.NewGuid(),
            OrganizerUserId = organizerId,
            VenueId = Guid.NewGuid(),
            StartAtUtc = DateTime.UtcNow.AddDays(2),
            EndAtUtc = DateTime.UtcNow.AddDays(3),
            Purpose = "Concert",
            OfferedAmount = 25000m,
            Status = RentalRequestStatus.Pending
        };

        var audit = new FakeAuditLogService();

        var service = new VenueRentalService(
            new FakeVenueRentalRepository(rental),
            new FakeVenueRepository(null),
            new FakeNotificationService(),
            null,
            audit);

        var result = await service.CancelAsync(
            organizerId,
            rental.Id);

        Assert.Equal(
            RentalRequestStatus.Cancelled,
            result.Status);

        var entry = Assert.Single(audit.Entries);

        Assert.Equal(
            organizerId,
            entry.ActorUserId);

        Assert.Equal(
            "Venue rental cancelled",
            entry.Action);

        Assert.Contains(
            "Pending",
            entry.BeforeSummary ?? string.Empty,
            StringComparison.OrdinalIgnoreCase);

        Assert.Contains(
            "Cancelled",
            entry.AfterSummary ?? string.Empty,
            StringComparison.OrdinalIgnoreCase);
    }

    private sealed class FakeAuditLogService
        : IAuditLogService
    {
        public List<AuditEntry> Entries { get; } = new();

        public Task WriteAsync(
            Guid? actorUserId,
            string action,
            string entityType,
            string? entityId,
            string? beforeSummary,
            string? afterSummary,
            string? correlationId,
            string? ipAddress,
            CancellationToken cancellationToken = default)
        {
            Entries.Add(
                new AuditEntry(
                    actorUserId,
                    action,
                    entityType,
                    entityId,
                    beforeSummary,
                    afterSummary));

            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<AuditLogResponse>> QueryAsync(
            AuditLogQuery query,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyList<AuditLogResponse>>(
                Array.Empty<AuditLogResponse>());
        }
    }

    private sealed record AuditEntry(
        Guid? ActorUserId,
        string Action,
        string EntityType,
        string? EntityId,
        string? BeforeSummary,
        string? AfterSummary);

    private sealed class FakeEventRepository(
        Event eventEntity)
        : IEventRepository
    {
        public Task<IReadOnlyList<Event>> GetPublishedAsync(
            EventSearchRequest request,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyList<Event>>(
                new[] { eventEntity });
        }

        public Task<IReadOnlyList<Event>> GetByOrganizerUserIdAsync(
            Guid organizerUserId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyList<Event>>(
                new[] { eventEntity });
        }

        public Task<Event?> GetByIdAsync(
            Guid eventId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<Event?>(
                eventEntity.Id == eventId
                    ? eventEntity
                    : null);
        }

        public Task AddAsync(
            Event value,
            CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task SaveChangesAsync(
            CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }

    private sealed class FakeVenueRepository(
        Venue? venue)
        : IVenueRepository
    {
        public Task<IReadOnlyList<Venue>> GetAllAsync(
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyList<Venue>>(
                venue is null
                    ? Array.Empty<Venue>()
                    : new[] { venue });
        }

        public Task<IReadOnlyList<Venue>> GetByOwnerUserIdAsync(
            Guid ownerUserId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyList<Venue>>(
                venue is not null &&
                venue.OwnerUserId == ownerUserId
                    ? new[] { venue }
                    : Array.Empty<Venue>());
        }

        public Task<Venue?> GetByIdAsync(
            Guid venueId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<Venue?>(
                venue is not null &&
                venue.Id == venueId
                    ? venue
                    : null);
        }

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
        private readonly VenueRentalRequest? rental;

        public FakeVenueRentalRepository(
            VenueRentalRequest? rental = null)
        {
            this.rental = rental;
        }

        public bool HasAcceptedRental { get; set; }

        public Task<VenueRentalRequest?> GetByIdAsync(
            Guid rentalId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<VenueRentalRequest?>(
                rental is not null &&
                rental.Id == rentalId
                    ? rental
                    : null);
        }

        public Task<IReadOnlyList<VenueRentalRequest>> GetByOrganizerAsync(
            Guid organizerUserId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyList<VenueRentalRequest>>(
                rental is not null &&
                rental.OrganizerUserId == organizerUserId
                    ? new[] { rental }
                    : Array.Empty<VenueRentalRequest>());
        }

        public Task<IReadOnlyList<VenueRentalRequest>> GetByVenueIdsAsync(
            IReadOnlyCollection<Guid> venueIds,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyList<VenueRentalRequest>>(
                rental is not null &&
                venueIds.Contains(rental.VenueId)
                    ? new[] { rental }
                    : Array.Empty<VenueRentalRequest>());
        }

        public Task<bool> HasAcceptedOverlapAsync(
            Guid venueId,
            DateTime startAtUtc,
            DateTime endAtUtc,
            Guid? excludeRentalId = null,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(false);
        }

        public Task<bool> HasAcceptedRentalForOrganizerAsync(
            Guid organizerUserId,
            Guid venueId,
            DateTime startAtUtc,
            DateTime endAtUtc,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(
                HasAcceptedRental);
        }

        public Task AddAsync(
            VenueRentalRequest request,
            CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task SaveChangesAsync(
            CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }

    private sealed class FakeNotificationService
        : INotificationService
    {
        public Task<IReadOnlyList<NotificationResponse>> GetMineAsync(
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyList<NotificationResponse>>(
                Array.Empty<NotificationResponse>());
        }

        public Task<NotificationResponse> MarkReadAsync(
            Guid userId,
            Guid notificationId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<NotificationResponse>(
                null!);
        }

        public Task<NotificationResponse> CreateAsync(
            Guid userId,
            string title,
            string message,
            string type,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<NotificationResponse>(
                null!);
        }
    }
}