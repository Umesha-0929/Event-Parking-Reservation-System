using SEVPMS.Application.Features.Seats.DTOs;
using SEVPMS.Application.Features.Seats.Interfaces;
using SEVPMS.Application.Features.Seats.Services;
using SEVPMS.Application.Features.Tickets.DTOs;
using SEVPMS.Application.Features.Tickets.Interfaces;
using Xunit;

namespace SEVPMS.UnitTests.Seats;

public sealed class SeatTicketFulfillmentServiceTests
{
    [Fact]
    public async Task CompletePaidBooking_CommitsHoldAndIssuesTickets()
    {
        var bookingId = Guid.NewGuid();
        var eventId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var seatId = Guid.NewGuid();

        var seats = new FakeSeatService
        {
            CommitResult = true
        };

        var tickets = new FakeTicketService();

        var service = new SeatTicketFulfillmentService(
            seats,
            tickets);

        var result = await service.CompletePaidBookingAsync(
            bookingId,
            eventId,
            customerId,
            "hold-token",
            new[] { seatId });

        Assert.True(seats.CommitCalled);
        Assert.Equal("hold-token", seats.HoldToken);
        Assert.Equal(customerId, seats.CustomerUserId);
        Assert.Equal(bookingId, seats.BookingId);

        Assert.True(tickets.IssueCalled);
        Assert.Equal(bookingId, tickets.BookingId);
        Assert.Equal(eventId, tickets.Request!.EventId);
        Assert.Single(tickets.Request.SeatIds);
        Assert.Equal(seatId, tickets.Request.SeatIds.Single());

        Assert.Single(result);
    }

    [Fact]
    public async Task CompletePaidBooking_DoesNotIssueTicketWhenHoldCommitFails()
    {
        var seats = new FakeSeatService
        {
            CommitResult = false
        };

        var tickets = new FakeTicketService();

        var service = new SeatTicketFulfillmentService(
            seats,
            tickets);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.CompletePaidBookingAsync(
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                "expired-hold",
                new[] { Guid.NewGuid() }));

        Assert.True(seats.CommitCalled);
        Assert.False(tickets.IssueCalled);
    }

    private sealed class FakeSeatService : ISeatService
    {
        public bool CommitResult { get; init; }
        public bool CommitCalled { get; private set; }
        public string? HoldToken { get; private set; }
        public Guid CustomerUserId { get; private set; }
        public Guid BookingId { get; private set; }

        public Task<bool> CommitHoldAsync(
            string holdToken,
            Guid userId,
            Guid bookingId,
            CancellationToken cancellationToken = default)
        {
            CommitCalled = true;
            HoldToken = holdToken;
            CustomerUserId = userId;
            BookingId = bookingId;

            return Task.FromResult(CommitResult);
        }

        public Task<IReadOnlyList<SeatAvailabilityDto>> GetAvailabilityAsync(
            Guid eventId,
            Guid? sectionId,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<SeatHoldResponse> HoldAsync(
            Guid eventId,
            Guid userId,
            CreateSeatHoldRequest request,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<bool> ReleaseHoldAsync(
            string holdToken,
            Guid userId,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<SeatAvailabilityDto> UpsertSeatAsync(
            Guid eventId,
            UpsertSeatRequest request,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<SeatViewAssetDto?> GetSeatViewAsync(
            Guid eventId,
            Guid seatId,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<SeatViewAssetDto> UpsertSeatViewAsync(
            Guid eventId,
            UpsertSeatViewAssetRequest request,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
    }

    private sealed class FakeTicketService : ITicketService
    {
        public bool IssueCalled { get; private set; }
        public Guid BookingId { get; private set; }
        public IssueTicketsRequest? Request { get; private set; }

        public Task<IReadOnlyList<TicketDto>> IssueAsync(
            Guid bookingId,
            IssueTicketsRequest request,
            CancellationToken cancellationToken = default)
        {
            IssueCalled = true;
            BookingId = bookingId;
            Request = request;

            IReadOnlyList<TicketDto> result =
                new[]
                {
                    new TicketDto(
                        Guid.NewGuid(),
                        "TKT-FINAL-001",
                        bookingId,
                        request.EventId,
                        request.SeatIds.FirstOrDefault(),
                        "Active",
                        DateTime.UtcNow,
                        "test-qr")
                };

            return Task.FromResult(result);
        }

        public Task<IReadOnlyList<TicketDto>> GetForBookingAsync(
            Guid bookingId,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<TicketDto?> GetByTicketNoAsync(
            string ticketNo,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<CheckInTicketResponse> CheckInAsync(
            Guid eventId,
            Guid scannerUserId,
            CheckInTicketRequest request,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<bool> CancelAsync(
            string ticketNo,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
    }
}
