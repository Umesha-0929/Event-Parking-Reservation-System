using SEVPMS.Application.Common.Exceptions;
using SEVPMS.Application.Features.Notifications.DTOs;
using SEVPMS.Application.Features.Notifications.Interfaces;
using SEVPMS.Application.Interfaces.Providers;
using SEVPMS.Application.Interfaces.Repositories;
using SEVPMS.Domain.Entities.Notifications;

namespace SEVPMS.Application.Features.Notifications.Services;

public sealed class NotificationService(
    INotificationRepository notificationRepository,
    INotificationRealtimePublisher realtimePublisher,
    IUserRepository? userRepository = null,
    IEmailSender? emailSender = null,
    ISmsSender? smsSender = null)
    : INotificationService
{
    public async Task<IReadOnlyList<NotificationResponse>> GetMineAsync(Guid userId, CancellationToken cancellationToken = default)
        => (await notificationRepository.GetByUserAsync(userId, cancellationToken)).Select(Map).ToList();

    public async Task<IReadOnlyList<NotificationResponse>> GetMinePageAsync(
        Guid userId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
        => (await notificationRepository.GetByUserPageAsync(
                userId, page, pageSize, cancellationToken))
            .Select(Map)
            .ToArray();

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

    public async Task<int> MarkAllReadAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var notifications = await notificationRepository.GetTrackedByUserAsync(userId, cancellationToken);
        var now = DateTime.UtcNow;
        var changed = 0;
        foreach (var notification in notifications.Where(x => !x.IsRead))
        {
            notification.IsRead = true;
            notification.ReadAtUtc = now;
            notification.UpdatedAtUtc = now;
            changed++;
        }
        if (changed > 0) await notificationRepository.SaveChangesAsync(cancellationToken);
        return changed;
    }

    public async Task DeleteAsync(Guid userId, Guid notificationId, CancellationToken cancellationToken = default)
    {
        var notification = await notificationRepository.GetByIdAsync(notificationId, cancellationToken)
            ?? throw new KeyNotFoundException("Notification was not found.");
        if (notification.UserId != userId)
            throw new ForbiddenAccessException("You do not have permission to clear this notification.");
        notificationRepository.Remove(notification);
        await notificationRepository.SaveChangesAsync(cancellationToken);
    }

    public async Task<int> ClearAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var notifications = await notificationRepository.GetTrackedByUserAsync(userId, cancellationToken);
        if (notifications.Count == 0) return 0;
        notificationRepository.RemoveRange(notifications);
        await notificationRepository.SaveChangesAsync(cancellationToken);
        return notifications.Count;
    }

    public async Task<NotificationResponse> CreateAsync(Guid userId, string title, string message, string type, CancellationToken cancellationToken = default)
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
        if (userRepository is not null)
        {
            var user = await userRepository.GetByIdAsync(userId, cancellationToken);
            if (user is not null)
            {
                if (emailSender is not null && !string.IsNullOrWhiteSpace(user.Email))
                {
                    try { await emailSender.SendAsync(user.Email, notification.Title, notification.Message, cancellationToken); }
                    catch { }
                }
                if (smsSender is not null && !string.IsNullOrWhiteSpace(user.PhoneNumber) && notification.Type is "Payment" or "Refund" or "VenueRental")
                {
                    try { await smsSender.SendAsync(user.PhoneNumber, $"{notification.Title}: {notification.Message}", cancellationToken); }
                    catch { }
                }
            }
        }
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
