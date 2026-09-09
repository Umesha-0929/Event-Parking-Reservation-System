using SEVPMS.Application.Features.Tickets.DTOs;
namespace SEVPMS.Application.Features.Tickets.Interfaces;
public interface ITicketService
{
    Task<IReadOnlyList<TicketDto>> IssueAsync(Guid bookingId, IssueTicketsRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TicketDto>> GetForBookingAsync(Guid bookingId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CustomerTicketSummaryDto>> GetMineAsync(Guid customerUserId, CancellationToken cancellationToken = default);
    async Task<IReadOnlyList<CustomerTicketSummaryDto>> GetMinePageAsync(
        Guid customerUserId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var all = await GetMineAsync(customerUserId, cancellationToken);
        var safePage = Math.Max(page, 1);
        var safePageSize = Math.Clamp(pageSize, 1, 200);
        return all.Skip((safePage - 1) * safePageSize).Take(safePageSize).ToArray();
    }
    Task<TicketDto?> GetByTicketNoAsync(string ticketNo, CancellationToken cancellationToken = default);
    Task<CheckInTicketResponse> CheckInAsync(Guid eventId, Guid scannerUserId, CheckInTicketRequest request, CancellationToken cancellationToken = default);
    Task<bool> CancelAsync(string ticketNo, CancellationToken cancellationToken = default);
}
