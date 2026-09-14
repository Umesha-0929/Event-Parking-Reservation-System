using SEVPMS.Domain.Entities.Tickets;
using SEVPMS.Domain.Enums;
namespace SEVPMS.Application.Features.Tickets.Interfaces;
public sealed record TicketCheckInAttempt(Ticket? Ticket, CheckInResult Result, DateTime ScannedAtUtc);
public sealed record CustomerTicketSummaryRow(
    Ticket Ticket,
    string BookingNumber,
    string EventName,
    string VenueName,
    string? RowLabel,
    string? SeatNumber,
    bool IsAccessible);
public interface ITicketRepository
{
    Task<IReadOnlyList<Ticket>> GetByBookingAsync(Guid bookingId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CustomerTicketSummaryRow>> GetForCustomerAsync(
        Guid customerUserId,
        CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<CustomerTicketSummaryRow>>(Array.Empty<CustomerTicketSummaryRow>());
    async Task<IReadOnlyList<CustomerTicketSummaryRow>> GetForCustomerPageAsync(
        Guid customerUserId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var all = await GetForCustomerAsync(customerUserId, cancellationToken);
        var safePage = Math.Max(page, 1);
        var safePageSize = Math.Clamp(pageSize, 1, 200);
        return all.Skip((safePage - 1) * safePageSize).Take(safePageSize).ToArray();
    }
    Task<IReadOnlyList<Ticket>> AddIfBookingHasNoneAsync(Guid bookingId, IReadOnlyCollection<Ticket> tickets, CancellationToken cancellationToken = default);
    Task<TicketCheckInAttempt> TryCheckInAsync(Guid ticketId, string qrTokenHash, Guid eventId, Guid scannedByUserId, string gate, DateTime nowUtc, CancellationToken cancellationToken = default);
    Task<Ticket?> GetByTicketNoAsync(string ticketNo, CancellationToken cancellationToken = default);
    Task<bool> CancelAsync(string ticketNo, DateTime nowUtc, CancellationToken cancellationToken = default);
}
