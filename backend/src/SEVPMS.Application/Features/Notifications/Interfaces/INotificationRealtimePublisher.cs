using SEVPMS.Application.Features.Notifications.DTOs;

namespace SEVPMS.Application.Features.Notifications.Interfaces;

public interface INotificationRealtimePublisher
{
    Task PublishAsync(Guid userId, NotificationResponse notification, CancellationToken cancellationToken = default);
}
