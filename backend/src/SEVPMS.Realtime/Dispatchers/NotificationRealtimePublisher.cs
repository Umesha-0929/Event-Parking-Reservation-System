using SEVPMS.Application.Features.Notifications.DTOs;
using SEVPMS.Application.Features.Notifications.Interfaces;
using SEVPMS.Realtime.Events;
using SEVPMS.Realtime.Groups;

namespace SEVPMS.Realtime.Dispatchers;

public sealed class NotificationRealtimePublisher(
    IRealtimeDispatcher dispatcher)
    : INotificationRealtimePublisher
{
    public Task PublishAsync(
        Guid userId,
        NotificationResponse notification,
        CancellationToken cancellationToken = default)
        => dispatcher.SendToGroupAsync(
            RealtimeGroupNames.User(userId),
            RealtimeEventNames.NotificationReceived,
            notification,
            cancellationToken);
}
