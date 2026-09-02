using SEVPMS.Application.Features.Notifications.DTOs;

namespace SEVPMS.Application.Features.Notifications.Interfaces;

public interface INotificationService
{
    Task<IReadOnlyList<NotificationResponse>> GetMineAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<NotificationResponse> MarkReadAsync(Guid userId, Guid notificationId, CancellationToken cancellationToken = default);
    Task<NotificationResponse> CreateAsync(Guid userId, string title, string message, string type, CancellationToken cancellationToken = default);
}
