namespace SEVPMS.Application.Features.Tickets.Interfaces;
public interface ITicketRealtimeNotifier
{
    Task PublishCheckInChangedAsync(Guid eventId, Guid ticketId, string state, string gate, DateTime scannedAtUtc, CancellationToken cancellationToken = default);
}
