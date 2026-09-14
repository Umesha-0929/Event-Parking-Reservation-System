using Microsoft.AspNetCore.SignalR;
using SEVPMS.Realtime.Hubs;

namespace SEVPMS.Realtime.Dispatchers;

public sealed class SignalRRealtimeDispatcher(IHubContext<NotificationHub> hubContext)
    : IRealtimeDispatcher
{
    public Task SendToGroupAsync(
        string groupName,
        string eventName,
        object payload,
        CancellationToken cancellationToken = default)
    {
        return hubContext.Clients
            .Group(groupName)
            .SendAsync(eventName, payload, cancellationToken);
    }
}
