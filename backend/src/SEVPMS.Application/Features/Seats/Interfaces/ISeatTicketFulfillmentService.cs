using SEVPMS.Application.Features.Tickets.DTOs;

namespace SEVPMS.Application.Features.Seats.Interfaces;

public interface ISeatTicketFulfillmentService
{
    Task<IReadOnlyCollection<TicketDto>> CompletePaidBookingAsync(
        Guid bookingId,
        Guid eventId,
        Guid customerUserId,
        string holdToken,
        IReadOnlyCollection<Guid> seatIds,
        CancellationToken cancellationToken = default);
}
