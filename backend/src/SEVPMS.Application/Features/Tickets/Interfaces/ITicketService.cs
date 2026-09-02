using SEVPMS.Application.Features.Tickets.DTOs;
namespace SEVPMS.Application.Features.Tickets.Interfaces;
public interface ITicketService
{
    Task<IReadOnlyList<TicketDto>> IssueAsync(Guid bookingId, IssueTicketsRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TicketDto>> GetForBookingAsync(Guid bookingId, CancellationToken cancellationToken = default);
    Task<TicketDto?> GetByTicketNoAsync(string ticketNo, CancellationToken cancellationToken = default);
    Task<CheckInTicketResponse> CheckInAsync(Guid eventId, Guid scannerUserId, CheckInTicketRequest request, CancellationToken cancellationToken = default);
    Task<bool> CancelAsync(string ticketNo, CancellationToken cancellationToken = default);
}
