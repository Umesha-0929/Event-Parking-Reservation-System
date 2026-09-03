using SEVPMS.Application.Features.Seats.Interfaces;
using SEVPMS.Application.Features.Tickets.Interfaces;
using SEVPMS.Realtime.Dispatchers;
using SEVPMS.Realtime.Events;
using SEVPMS.Realtime.Groups;

namespace SEVPMS.Api.Klegar;

public sealed class KlegarRealtimeNotifier(
    IRealtimeDispatcher dispatcher)
    : ISeatRealtimeNotifier,
      ITicketRealtimeNotifier
{
    public Task PublishSeatStateChangedAsync(
        Guid eventId,
        IReadOnlyCollection<Guid> seatIds,
        string state,
        DateTime? expiresAtUtc,
        CancellationToken cancellationToken = default)
        => dispatcher.SendToGroupAsync(
            RealtimeGroupNames.Event(eventId),
            RealtimeEventNames.SeatAvailabilityChanged,
            new
            {
                eventId,
                seatIds,
                state,
                expiresAtUtc
            },
            cancellationToken);

    public Task PublishCheckInChangedAsync(
        Guid eventId,
        Guid ticketId,
        string state,
        string gate,
        DateTime scannedAtUtc,
        CancellationToken cancellationToken = default)
        => dispatcher.SendToGroupAsync(
            RealtimeGroupNames.EventStaff(eventId),
            "checkin.changed",
            new
            {
                eventId,
                ticketId,
                state,
                gate,
                scannedAtUtc
            },
            cancellationToken);
}