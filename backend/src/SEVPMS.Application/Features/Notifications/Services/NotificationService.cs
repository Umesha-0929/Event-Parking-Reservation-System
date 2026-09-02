using SEVPMS.Application.Common.Exceptions;
using SEVPMS.Application.Features.Notifications.DTOs;
using SEVPMS.Application.Features.Notifications.Interfaces;
using SEVPMS.Application.Interfaces.Repositories;
using SEVPMS.Domain.Entities.Notifications;

namespace SEVPMS.Application.Features.Notifications.Services;

public sealed class NotificationService(
    INotificationRepository notificationRepository,
    INotificationRealtimePublisher realtimePublisher)
    : INotificationService
{
    public async Task<IReadOnlyList<NotificationResponse>> GetMineAsync(Guid userId, CancellationToken cancellationToken = default)
        => (await notificationRepository.GetByUserAsync(userId, cancellationToken)).Select(Map).ToList();

    public async Task<NotificationResponse> MarkReadAsync(Guid userId, Guid notificationId, CancellationToken cancellationToken = default)
    {
        var notification = await notificationRepository.GetByIdAsync(notificationId, cancellationToken)
            ?? throw new KeyNotFoundException("Notification was not found.");

        if (notification.UserId != userId)
            throw new ForbiddenAccessException("You do not have permission to read this notification.");

        if (!notification.IsRead)
        {
            notification.IsRead = true;
            notification.ReadAtUtc = DateTime.UtcNow;
            notification.UpdatedAtUtc = DateTime.UtcNow;
            await notificationRepository.SaveChangesAsync(cancellationToken);
        }

        return Map(notification);
    }

    public async Task<NotificationResponse> CreateAsync(
        Guid userId,
        string title,
        string message,
        string type,
        CancellationToken cancellationToken = default)
    {
        var notification = new Notification
        {
            UserId = userId,
            Title = string.IsNullOrWhiteSpace(title) ? "SEVPMS" : title.Trim(),
            Message = message?.Trim() ?? string.Empty,
            Type = string.IsNullOrWhiteSpace(type) ? "General" : type.Trim()
        };

        await notificationRepository.AddAsync(notification, cancellationToken);
        await notificationRepository.SaveChangesAsync(cancellationToken);

        var response = Map(notification);
        await realtimePublisher.PublishAsync(userId, response, cancellationToken);
        return response;
    }

    private static NotificationResponse Map(Notification x) => new()
    {
        NotificationId = x.Id,
        UserId = x.UserId,
        Title = x.Title,
        Message = x.Message,
        Type = x.Type,
        IsRead = x.IsRead,
        ReadAtUtc = x.ReadAtUtc,
        CreatedAtUtc = x.CreatedAtUtc
    };
}
