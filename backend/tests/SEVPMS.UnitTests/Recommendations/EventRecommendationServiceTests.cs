using SEVPMS.Application.Features.Events.DTOs;
using SEVPMS.Application.Features.Recommendations.DTOs;
using SEVPMS.Application.Features.Recommendations.Services;
using SEVPMS.Application.Interfaces.Repositories;
using SEVPMS.Domain.Entities.Bookings;
using SEVPMS.Domain.Entities.Events;
using SEVPMS.Domain.Enums;
using Xunit;

namespace SEVPMS.UnitTests.Recommendations;

public sealed class EventRecommendationServiceTests
{
    [Fact]
    public async Task GetRecommendationsAsync_PrefersRequestedCategory()
    {
        var customerId = Guid.NewGuid();

        var musicEvent =
            CreateEvent(
                "Music Night",
                "Music",
                DateTime.UtcNow.AddDays(3));

        var sportsEvent =
            CreateEvent(
                "Football Match",
                "Sports",
                DateTime.UtcNow.AddDays(1));

        var service =
            CreateService(
                [musicEvent, sportsEvent],
                []);

        var result =
            await service.GetRecommendationsAsync(
                customerId,
                new EventRecommendationRequest
                {
                    PreferredCategories =
                        ["Music"],
                    Limit =
                        10
                });

        Assert.Equal(
            2,
            result.Count);

        Assert.Equal(
            musicEvent.Id,
            result[0].EventId);

        Assert.Equal(
            5,
            result[0].RecommendationScore);

        Assert.Contains(
            result[0].Reasons,
            reason =>
                reason.Contains(
                    "preferred",
                    StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task GetRecommendationsAsync_UsesBookingHistoryCategory()
    {
        var customerId = Guid.NewGuid();

        var historicalEvent =
            CreateEvent(
                "Old Concert",
                "Music",
                DateTime.UtcNow.AddDays(-10));

        historicalEvent.Status =
            EventStatus.Completed;

        var futureMusicEvent =
            CreateEvent(
                "Future Concert",
                "Music",
                DateTime.UtcNow.AddDays(5));

        var futureFoodEvent =
            CreateEvent(
                "Food Festival",
                "Food",
                DateTime.UtcNow.AddDays(2));

        var booking =
            CreateBooking(
                customerId,
                historicalEvent.Id,
                BookingStatus.Completed);

        var service =
            CreateService(
                [
                    historicalEvent,
                    futureMusicEvent,
                    futureFoodEvent
                ],
                [booking]);

        var result =
            await service.GetRecommendationsAsync(
                customerId,
                new EventRecommendationRequest());

        Assert.Equal(
            futureMusicEvent.Id,
            result[0].EventId);

        Assert.Equal(
            2,
            result[0].RecommendationScore);

        Assert.Contains(
            result[0].Reasons,
            reason =>
                reason.Contains(
                    "booked before",
                    StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task GetRecommendationsAsync_ExcludesAlreadyBookedEvents()
    {
        var customerId = Guid.NewGuid();

        var bookedEvent =
            CreateEvent(
                "Booked Event",
                "Music",
                DateTime.UtcNow.AddDays(2));

        var availableEvent =
            CreateEvent(
                "Available Event",
                "Music",
                DateTime.UtcNow.AddDays(3));

        var booking =
            CreateBooking(
                customerId,
                bookedEvent.Id,
                BookingStatus.Confirmed);

        var service =
            CreateService(
                [
                    bookedEvent,
                    availableEvent
                ],
                [booking]);

        var result =
            await service.GetRecommendationsAsync(
                customerId,
                new EventRecommendationRequest
                {
                    PreferredCategories =
                        ["Music"]
                });

        Assert.Single(
            result);

        Assert.Equal(
            availableEvent.Id,
            result[0].EventId);
    }

    [Fact]
    public async Task GetRecommendationsAsync_OnlyReturnsUpcomingPublishedEvents()
    {
        var customerId = Guid.NewGuid();

        var upcoming =
            CreateEvent(
                "Upcoming",
                "Music",
                DateTime.UtcNow.AddDays(2));

        var past =
            CreateEvent(
                "Past",
                "Music",
                DateTime.UtcNow.AddDays(-2));

        var cancelled =
            CreateEvent(
                "Cancelled",
                "Music",
                DateTime.UtcNow.AddDays(4));

        cancelled.Status =
            EventStatus.Cancelled;

        var service =
            CreateService(
                [
                    upcoming,
                    past,
                    cancelled
                ],
                []);

        var result =
            await service.GetRecommendationsAsync(
                customerId,
                new EventRecommendationRequest());

        Assert.Single(
            result);

        Assert.Equal(
            upcoming.Id,
            result[0].EventId);
    }

    [Fact]
    public async Task GetRecommendationsAsync_RespectsLimit()
    {
        var customerId = Guid.NewGuid();

        var events =
            Enumerable
                .Range(
                    1,
                    5)
                .Select(index =>
                    CreateEvent(
                        $"Event {index}",
                        "Music",
                        DateTime.UtcNow.AddDays(index)))
                .ToList();

        var service =
            CreateService(
                events,
                []);

        var result =
            await service.GetRecommendationsAsync(
                customerId,
                new EventRecommendationRequest
                {
                    Limit =
                        2
                });

        Assert.Equal(
            2,
            result.Count);
    }

    [Fact]
    public async Task GetRecommendationsAsync_WithInvalidLimit_Throws()
    {
        var service =
            CreateService(
                [],
                []);

        await Assert.ThrowsAsync<ArgumentException>(
            () =>
                service.GetRecommendationsAsync(
                    Guid.NewGuid(),
                    new EventRecommendationRequest
                    {
                        Limit =
                            0
                    }));
    }

    private static EventRecommendationService
        CreateService(
            IReadOnlyList<Event> events,
            IReadOnlyList<Booking> bookings)
        => new(
            new FakeEventRepository(events),
            new FakeBookingRepository(bookings));

    private static Event CreateEvent(
        string title,
        string category,
        DateTime startAtUtc)
        => new()
        {
            OrganizerUserId =
                Guid.NewGuid(),
            VenueId =
                Guid.NewGuid(),
            Title =
                title,
            Description =
                $"{title} description",
            Category =
                category,
            StartAtUtc =
                startAtUtc,
            EndAtUtc =
                startAtUtc.AddHours(3),
            Status =
                EventStatus.Published
        };

    private static Booking CreateBooking(
        Guid customerId,
        Guid eventId,
        BookingStatus status)
        => new()
        {
            BookingNumber =
                $"BKG-{Guid.NewGuid():N}",
            CustomerUserId =
                customerId,
            EventId =
                eventId,
            HoldToken =
                Guid.NewGuid().ToString("N"),
            TotalAmount =
                1000m,
            Status =
                status
        };

    private sealed class FakeEventRepository(
        IReadOnlyList<Event> events)
        : IEventRepository
    {
        public Task<IReadOnlyList<Event>>
            GetPublishedAsync(
                EventSearchRequest request,
                CancellationToken cancellationToken = default)
            => Task.FromResult<
                IReadOnlyList<Event>>(
                events
                    .Where(eventEntity =>
                        eventEntity.Status ==
                        EventStatus.Published)
                    .ToList());

        public Task<IReadOnlyList<Event>>
            GetByOrganizerUserIdAsync(
                Guid organizerUserId,
                CancellationToken cancellationToken = default)
            => Task.FromResult<
                IReadOnlyList<Event>>(
                events
                    .Where(eventEntity =>
                        eventEntity.OrganizerUserId ==
                        organizerUserId)
                    .ToList());

        public Task<Event?> GetByIdAsync(
            Guid eventId,
            CancellationToken cancellationToken = default)
            => Task.FromResult(
                events.SingleOrDefault(
                    eventEntity =>
                        eventEntity.Id ==
                        eventId));

        public Task AddAsync(
            Event eventEntity,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task SaveChangesAsync(
            CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }

    private sealed class FakeBookingRepository(
        IReadOnlyList<Booking> bookings)
        : IBookingRepository
    {
        public Task<Booking?> GetByIdAsync(
            Guid bookingId,
            CancellationToken cancellationToken = default)
            => Task.FromResult(
                bookings.SingleOrDefault(
                    booking =>
                        booking.Id ==
                        bookingId));

        public Task<IReadOnlyList<Booking>>
            GetByCustomerAsync(
                Guid customerUserId,
                CancellationToken cancellationToken = default)
            => Task.FromResult<
                IReadOnlyList<Booking>>(
                bookings
                    .Where(booking =>
                        booking.CustomerUserId ==
                        customerUserId)
                    .ToList());

        public Task<IReadOnlyList<Guid>>
            GetSeatIdsAsync(
                Guid bookingId,
                CancellationToken cancellationToken = default)
            => Task.FromResult<
                IReadOnlyList<Guid>>(
                Array.Empty<Guid>());

        public Task AddAsync(
            Booking booking,
            IReadOnlyCollection<BookingSeat> bookingSeats,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task SaveChangesAsync(
            CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }
}