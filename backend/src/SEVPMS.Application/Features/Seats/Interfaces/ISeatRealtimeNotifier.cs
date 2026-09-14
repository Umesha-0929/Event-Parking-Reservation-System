namespace SEVPMS.Application.Features.Seats.Interfaces;
public interface ISeatRealtimeNotifier
{
    Task PublishSeatStateChangedAsync(Guid eventId, IReadOnlyCollection<Guid> seatIds, string state, DateTime? expiresAtUtc, CancellationToken cancellationToken = default);
}
