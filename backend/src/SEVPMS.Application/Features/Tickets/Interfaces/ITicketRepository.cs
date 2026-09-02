using SEVPMS.Domain.Entities.Tickets;
using SEVPMS.Domain.Enums;
namespace SEVPMS.Application.Features.Tickets.Interfaces;
public sealed record TicketCheckInAttempt(Ticket? Ticket, CheckInResult Result, DateTime ScannedAtUtc);
public interface ITicketRepository
{
    Task<IReadOnlyList<Ticket>> GetByBookingAsync(Guid bookingId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Ticket>> AddIfBookingHasNoneAsync(Guid bookingId, IReadOnlyCollection<Ticket> tickets, CancellationToken cancellationToken = default);
    Task<TicketCheckInAttempt> TryCheckInAsync(Guid ticketId, string qrTokenHash, Guid eventId, Guid scannedByUserId, string gate, DateTime nowUtc, CancellationToken cancellationToken = default);
    Task<Ticket?> GetByTicketNoAsync(string ticketNo, CancellationToken cancellationToken = default);
    Task<bool> CancelAsync(string ticketNo, DateTime nowUtc, CancellationToken cancellationToken = default);
}
