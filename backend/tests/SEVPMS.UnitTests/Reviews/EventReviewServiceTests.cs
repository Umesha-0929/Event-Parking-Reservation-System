using SEVPMS.Application.Features.Reviews.DTOs;
using SEVPMS.Application.Features.Reviews.Interfaces;
using SEVPMS.Application.Features.Reviews.Services;
using SEVPMS.Application.Features.Tickets.Interfaces;
using SEVPMS.Application.Interfaces.Repositories;
using SEVPMS.Domain.Entities.Bookings;
using SEVPMS.Domain.Entities.Reviews;
using SEVPMS.Domain.Entities.Tickets;
using SEVPMS.Domain.Enums;
using Xunit;

namespace SEVPMS.UnitTests.Reviews;

public sealed class EventReviewServiceTests
{
    [Fact]
    public async Task CreateAsync_WithCompletedBooking_CreatesReview()
    {
        var customerId = Guid.NewGuid();
        var eventId = Guid.NewGuid();

        var booking = CreateBooking(
            customerId,
            eventId,
            BookingStatus.Completed);

        var reviewRepository =
            new FakeEventReviewRepository();

        var service =
            CreateService(
                reviewRepository,
                booking,
                []);

        var result =
            await service.CreateAsync(
                customerId,
                eventId,
                new CreateEventReviewRequest
                {
                    BookingId = booking.Id,
                    Rating = 5,
                    Comment = "Great event"
                });

        Assert.Equal(
            eventId,
            result.EventId);

        Assert.Equal(
            customerId,
            result.CustomerUserId);

        Assert.Equal(
            booking.Id,
            result.BookingId);

        Assert.Equal(
            5,
            result.Rating);

        Assert.Equal(
            "Great event",
            result.Comment);

        Assert.Single(
            reviewRepository.Reviews);
    }

    [Fact]
    public async Task CreateAsync_WithCheckedInTicket_CreatesReview()
    {
        var customerId = Guid.NewGuid();
        var eventId = Guid.NewGuid();

        var booking = CreateBooking(
            customerId,
            eventId,
            BookingStatus.Confirmed);

        var ticket =
            new Ticket
            {
                BookingId =
                    booking.Id,
                EventId =
                    eventId,
                TicketNo =
                    "TKT-001",
                QrTokenHash =
                    "hash",
                IssuedAtUtc =
                    DateTime.UtcNow.AddHours(-2),
                CheckedInAtUtc =
                    DateTime.UtcNow.AddHours(-1)
            };

        var reviewRepository =
            new FakeEventReviewRepository();

        var service =
            CreateService(
                reviewRepository,
                booking,
                [ticket]);

        var result =
            await service.CreateAsync(
                customerId,
                eventId,
                new CreateEventReviewRequest
                {
                    BookingId =
                        booking.Id,
                    Rating =
                        4,
                    Comment =
                        "Very good"
                });

        Assert.Equal(
            4,
            result.Rating);

        Assert.Single(
            reviewRepository.Reviews);
    }

    [Fact]
    public async Task CreateAsync_WhenNotVerifiedOrCompleted_Throws()
    {
        var customerId = Guid.NewGuid();
        var eventId = Guid.NewGuid();

        var booking = CreateBooking(
            customerId,
            eventId,
            BookingStatus.Pending);

        var service =
            CreateService(
                new FakeEventReviewRepository(),
                booking,
                []);

        var exception =
            await Assert.ThrowsAsync<InvalidOperationException>(
                () =>
                    service.CreateAsync(
                        customerId,
                        eventId,
                        new CreateEventReviewRequest
                        {
                            BookingId =
                                booking.Id,
                            Rating =
                                4
                        }));

        Assert.Contains(
            "verified attendees",
            exception.Message,
            StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CreateAsync_WhenReviewAlreadyExists_Throws()
    {
        var customerId = Guid.NewGuid();
        var eventId = Guid.NewGuid();

        var booking = CreateBooking(
            customerId,
            eventId,
            BookingStatus.Completed);

        var reviewRepository =
            new FakeEventReviewRepository();

        reviewRepository.Reviews.Add(
            new EventReview
            {
                EventId =
                    eventId,
                CustomerUserId =
                    customerId,
                BookingId =
                    booking.Id,
                Rating =
                    5,
                Comment =
                    "Existing review"
            });

        var service =
            CreateService(
                reviewRepository,
                booking,
                []);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () =>
                service.CreateAsync(
                    customerId,
                    eventId,
                    new CreateEventReviewRequest
                    {
                        BookingId =
                            booking.Id,
                        Rating =
                            4
                    }));
    }

    [Fact]
    public async Task GetSummaryAsync_ReturnsAverageAndCount()
    {
        var eventId = Guid.NewGuid();

        var reviewRepository =
            new FakeEventReviewRepository();

        reviewRepository.Reviews.AddRange(
            new[]
            {
                new EventReview
                {
                    EventId = eventId,
                    CustomerUserId = Guid.NewGuid(),
                    BookingId = Guid.NewGuid(),
                    Rating = 5
                },
                new EventReview
                {
                    EventId = eventId,
                    CustomerUserId = Guid.NewGuid(),
                    BookingId = Guid.NewGuid(),
                    Rating = 4
                },
                new EventReview
                {
                    EventId = eventId,
                    CustomerUserId = Guid.NewGuid(),
                    BookingId = Guid.NewGuid(),
                    Rating = 3
                }
            });

        var service =
            new EventReviewService(
                reviewRepository,
                new FakeBookingRepository(null),
                new FakeTicketRepository([]));

        var result =
            await service.GetSummaryAsync(
                eventId);

        Assert.Equal(
            eventId,
            result.EventId);

        Assert.Equal(
            3,
            result.ReviewCount);

        Assert.Equal(
            4,
            result.AverageRating);
    }

    [Fact]
    public async Task CreateAsync_WithInvalidRating_Throws()
    {
        var customerId = Guid.NewGuid();
        var eventId = Guid.NewGuid();

        var booking = CreateBooking(
            customerId,
            eventId,
            BookingStatus.Completed);

        var service =
            CreateService(
                new FakeEventReviewRepository(),
                booking,
                []);

        await Assert.ThrowsAsync<ArgumentException>(
            () =>
                service.CreateAsync(
                    customerId,
                    eventId,
                    new CreateEventReviewRequest
                    {
                        BookingId =
                            booking.Id,
                        Rating =
                            6
                    }));
    }

    private static EventReviewService CreateService(
        FakeEventReviewRepository reviewRepository,
        Booking booking,
        IReadOnlyList<Ticket> tickets)
        => new(
            reviewRepository,
            new FakeBookingRepository(booking),
            new FakeTicketRepository(tickets));

    private static Booking CreateBooking(
        Guid customerId,
        Guid eventId,
        BookingStatus status)
        => new()
        {
            BookingNumber =
                "BKG-TEST-001",
            CustomerUserId =
                customerId,
            EventId =
                eventId,
            HoldToken =
                "hold-test",
            TotalAmount =
                1000m,
            Status =
                status
        };

    private sealed class FakeEventReviewRepository
        : IEventReviewRepository
    {
        public List<EventReview> Reviews { get; }
            = new();

        public Task<EventReview?>
            GetByEventAndCustomerAsync(
                Guid eventId,
                Guid customerUserId,
                CancellationToken cancellationToken = default)
            => Task.FromResult(
                Reviews.SingleOrDefault(
                    x =>
                        x.EventId == eventId &&
                        x.CustomerUserId ==
                        customerUserId));

        public Task<IReadOnlyList<EventReview>>
            GetByEventAsync(
                Guid eventId,
                CancellationToken cancellationToken = default)
            => Task.FromResult<
                IReadOnlyList<EventReview>>(
                Reviews
                    .Where(
                        x => x.EventId == eventId)
                    .ToList());

        public Task AddAsync(
            EventReview review,
            CancellationToken cancellationToken = default)
        {
            Reviews.Add(review);

            return Task.CompletedTask;
        }

        public Task SaveChangesAsync(
            CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }

    private sealed class FakeBookingRepository(
        Booking? booking)
        : IBookingRepository
    {
        public Task<Booking?> GetByIdAsync(
            Guid bookingId,
            CancellationToken cancellationToken = default)
            => Task.FromResult(
                booking is not null &&
                booking.Id == bookingId
                    ? booking
                    : null);

        public Task<IReadOnlyList<Booking>>
            GetByCustomerAsync(
                Guid customerUserId,
                CancellationToken cancellationToken = default)
            => Task.FromResult<
                IReadOnlyList<Booking>>(
                booking is not null &&
                booking.CustomerUserId ==
                customerUserId
                    ? new[] { booking }
                    : Array.Empty<Booking>());

        public Task<IReadOnlyList<Guid>>
            GetSeatIdsAsync(
                Guid bookingId,
                CancellationToken cancellationToken = default)
            => Task.FromResult<
                IReadOnlyList<Guid>>(
                Array.Empty<Guid>());

        public Task AddAsync(
            Booking newBooking,
            IReadOnlyCollection<BookingSeat> bookingSeats,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task SaveChangesAsync(
            CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }

    private sealed class FakeTicketRepository(
        IReadOnlyList<Ticket> tickets)
        : ITicketRepository
    {
        public Task<IReadOnlyList<Ticket>>
            GetByBookingAsync(
                Guid bookingId,
                CancellationToken cancellationToken = default)
            => Task.FromResult<
                IReadOnlyList<Ticket>>(
                tickets
                    .Where(
                        x => x.BookingId == bookingId)
                    .ToList());

        public Task<IReadOnlyList<Ticket>>
            AddIfBookingHasNoneAsync(
                Guid bookingId,
                IReadOnlyCollection<Ticket> newTickets,
                CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<TicketCheckInAttempt>
            TryCheckInAsync(
                Guid ticketId,
                string qrTokenHash,
                Guid eventId,
                Guid scannedByUserId,
                string gate,
                DateTime nowUtc,
                CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<Ticket?> GetByTicketNoAsync(
            string ticketNo,
            CancellationToken cancellationToken = default)
            => Task.FromResult<Ticket?>(
                tickets.SingleOrDefault(
                    x => x.TicketNo == ticketNo));

        public Task<bool> CancelAsync(
            string ticketNo,
            DateTime nowUtc,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
    }
}