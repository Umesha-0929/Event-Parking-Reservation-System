using SEVPMS.Application.Features.Seats.Interfaces;
using SEVPMS.Application.Features.Tickets.DTOs;
using SEVPMS.Application.Features.Tickets.Interfaces;

namespace SEVPMS.Application.Features.Seats.Services;

public sealed class SeatTicketFulfillmentService(
    ISeatService seatService,
    ITicketService ticketService)
    : ISeatTicketFulfillmentService
{
    public async Task<IReadOnlyCollection<TicketDto>> CompletePaidBookingAsync(
        Guid bookingId,
        Guid eventId,
        Guid customerUserId,
        string holdToken,
        IReadOnlyCollection<Guid> seatIds,
        CancellationToken cancellationToken = default)
    {
        if (bookingId == Guid.Empty)
            throw new ArgumentException("Booking id is required.");

        if (eventId == Guid.Empty)
            throw new ArgumentException("Event id is required.");

        if (customerUserId == Guid.Empty)
            throw new ArgumentException("Customer user id is required.");

        if (string.IsNullOrWhiteSpace(holdToken))
            throw new ArgumentException("Seat hold token is required.");

        if (seatIds is null || seatIds.Count == 0)
            throw new ArgumentException(
                "At least one booked seat is required.");

        var committed =
            await seatService.CommitHoldAsync(
                holdToken,
                customerUserId,
                bookingId,
                cancellationToken);

        if (!committed)
        {
            throw new InvalidOperationException(
                "Seat hold could not be converted to a booking.");
        }

        var request =
            new IssueTicketsRequest(
                eventId,
                seatIds
                    .Distinct()
                    .Select(x => (Guid?)x)
                    .ToArray());

        return await ticketService.IssueAsync(
            bookingId,
            request,
            cancellationToken);
    }
}
