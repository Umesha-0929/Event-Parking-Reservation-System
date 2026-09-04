using SEVPMS.Application.Common.Exceptions;
using SEVPMS.Application.Features.Calendar.Services;
using SEVPMS.Application.Features.Events.DTOs;
using SEVPMS.Application.Interfaces.Repositories;
using SEVPMS.Domain.Entities.Bookings;
using SEVPMS.Domain.Entities.Events;
using SEVPMS.Domain.Entities.Venues;
using SEVPMS.Domain.Enums;
using Xunit;

namespace SEVPMS.UnitTests.Calendar;

public sealed class BookingCalendarServiceTests
{
    [Fact]
    public async Task GetAsync_returns_google_link_and_ics_for_confirmed_booking()
    {
        var customerId = Guid.NewGuid();

        var venue = new Venue
        {
            Id = Guid.NewGuid(),
            OwnerUserId = Guid.NewGuid(),
            Name = "Colombo Convention Centre",
            Description = "Test venue",
            AddressLine1 = "100 Main Road",
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
            Title = "SEVPMS Tech Expo",
            Description = "Annual technology event",
            Category = "Exhibition",
            StartAtUtc = new DateTime(
                2026, 10, 10,
                9, 0, 0,
                DateTimeKind.Utc),
            EndAtUtc = new DateTime(
                2026, 10, 10,
                17, 0, 0,
                DateTimeKind.Utc),
            Status = EventStatus.Published
        };

        var booking = new Booking
        {
            Id = Guid.NewGuid(),
            BookingNumber = "BKG-10001",
            CustomerUserId = customerId,
            EventId = eventEntity.Id,
            HoldToken = "TEST-HOLD",
            TotalAmount = 5000m,
            Status = BookingStatus.Confirmed,
            ConfirmedAtUtc = DateTime.UtcNow
        };

        var service = new BookingCalendarService(
            new FakeBookingRepository(booking),
            new FakeEventRepository(eventEntity),
            new FakeVenueRepository(venue));

        var result = await service.GetAsync(
            customerId,
            booking.Id);

        Assert.Equal(
            booking.Id,
            result.Info.BookingId);

        Assert.Equal(
            eventEntity.Id,
            result.Info.EventId);

        Assert.Equal(
            "SEVPMS Tech Expo",
            result.Info.EventTitle);

        Assert.Contains(
            "calendar.google.com/calendar/render",
            result.Info.GoogleCalendarUrl);

        Assert.Contains(
            "20261010T090000Z",
            result.Info.GoogleCalendarUrl);

        Assert.Equal(
            $"/api/bookings/{booking.Id}/calendar.ics",
            result.Info.IcsDownloadPath);

        Assert.EndsWith(
            ".ics",
            result.FileName);

        Assert.Contains(
            "BEGIN:VCALENDAR",
            result.IcsContent);

        Assert.Contains(
            "BEGIN:VEVENT",
            result.IcsContent);

        Assert.Contains(
            "DTSTART:20261010T090000Z",
            result.IcsContent);

        Assert.Contains(
            "DTEND:20261010T170000Z",
            result.IcsContent);

        Assert.Contains(
            "SUMMARY:SEVPMS Tech Expo",
            result.IcsContent);

        Assert.Contains(
            "STATUS:CONFIRMED",
            result.IcsContent);

        Assert.Contains(
            "END:VCALENDAR",
            result.IcsContent);
    }

    [Fact]
    public async Task GetAsync_rejects_non_confirmed_booking()
    {
        var customerId = Guid.NewGuid();

        var booking = new Booking
        {
            Id = Guid.NewGuid(),
            BookingNumber = "BKG-PENDING",
            CustomerUserId = customerId,
            EventId = Guid.NewGuid(),
            HoldToken = "TEST",
            TotalAmount = 1000m,
            Status = BookingStatus.Pending
        };

        var service = new BookingCalendarService(
            new FakeBookingRepository(booking),
            new FakeEventRepository(null),
            new FakeVenueRepository(null));

        var exception =
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.GetAsync(
                    customerId,
                    booking.Id));

        Assert.Contains(
            "confirmed bookings",
            exception.Message,
            StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GetAsync_rejects_booking_owned_by_another_customer()
    {
        var ownerId = Guid.NewGuid();
        var differentCustomerId = Guid.NewGuid();

        var booking = new Booking
        {
            Id = Guid.NewGuid(),
            BookingNumber = "BKG-PRIVATE",
            CustomerUserId = ownerId,
            EventId = Guid.NewGuid(),
            HoldToken = "TEST",
            TotalAmount = 1500m,
            Status = BookingStatus.Confirmed
        };

        var service = new BookingCalendarService(
            new FakeBookingRepository(booking),
            new FakeEventRepository(null),
            new FakeVenueRepository(null));

        await Assert.ThrowsAsync<ForbiddenAccessException>(
            () => service.GetAsync(
                differentCustomerId,
                booking.Id));
    }

    private sealed class FakeBookingRepository
        : IBookingRepository
    {
        private readonly Booking booking;

        public FakeBookingRepository(
            Booking booking)
        {
            this.booking = booking;
        }

        public Task<Booking?> GetByIdAsync(
            Guid bookingId,
            CancellationToken cancellationToken = default)
            => Task.FromResult<Booking?>(
                booking.Id == bookingId
                    ? booking
                    : null);

        public Task<IReadOnlyList<Booking>>
            GetByCustomerAsync(
                Guid customerUserId,
                CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<Booking>>(
                booking.CustomerUserId == customerUserId
                    ? new[] { booking }
                    : Array.Empty<Booking>());

        public Task<IReadOnlyList<Guid>>
            GetSeatIdsAsync(
                Guid bookingId,
                CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<Guid>>(
                Array.Empty<Guid>());

        public Task AddAsync(
            Booking booking,
            IReadOnlyCollection<BookingSeat> bookingSeats,
            CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task SaveChangesAsync(
            CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }

    private sealed class FakeEventRepository
        : IEventRepository
    {
        private readonly Event? eventEntity;

        public FakeEventRepository(
            Event? eventEntity)
        {
            this.eventEntity = eventEntity;
        }

        public Task<Event?> GetByIdAsync(
            Guid eventId,
            CancellationToken cancellationToken = default)
            => Task.FromResult(
                eventEntity is not null &&
                eventEntity.Id == eventId
                    ? eventEntity
                    : null);

        public Task<IReadOnlyList<Event>>
            GetPublishedAsync(
                EventSearchRequest request,
                CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<Event>>(
                eventEntity is null
                    ? Array.Empty<Event>()
                    : new[] { eventEntity });

        public Task<IReadOnlyList<Event>>
            GetByOrganizerUserIdAsync(
                Guid organizerUserId,
                CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<Event>>(
                eventEntity is not null &&
                eventEntity.OrganizerUserId == organizerUserId
                    ? new[] { eventEntity }
                    : Array.Empty<Event>());

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
        private readonly Venue? venue;

        public FakeVenueRepository(
            Venue? venue)
        {
            this.venue = venue;
        }

        public Task<IReadOnlyList<Venue>> GetAllAsync(
            CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<Venue>>(
                venue is null
                    ? Array.Empty<Venue>()
                    : new[] { venue });

        public Task<IReadOnlyList<Venue>>
            GetByOwnerUserIdAsync(
                Guid ownerUserId,
                CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<Venue>>(
                venue is not null &&
                venue.OwnerUserId == ownerUserId
                    ? new[] { venue }
                    : Array.Empty<Venue>());

        public Task<Venue?> GetByIdAsync(
            Guid venueId,
            CancellationToken cancellationToken = default)
            => Task.FromResult(
                venue is not null &&
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