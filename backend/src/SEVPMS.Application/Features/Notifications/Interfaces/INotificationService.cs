using SEVPMS.Application.Features.Notifications.DTOs;

namespace SEVPMS.Application.Features.Notifications.Interfaces;

public interface INotificationService
{
    Task<IReadOnlyList<NotificationResponse>> GetMineAsync(Guid userId, CancellationToken cancellationToken = default);
    async Task<IReadOnlyList<NotificationResponse>> GetMinePageAsync(
        Guid userId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var all = await GetMineAsync(userId, cancellationToken);
        var safePage = Math.Max(page, 1);
        var safePageSize = Math.Clamp(pageSize, 1, 200);
        return all.Skip((safePage - 1) * safePageSize).Take(safePageSize).ToArray();
    }
    Task<NotificationResponse> MarkReadAsync(Guid userId, Guid notificationId, CancellationToken cancellationToken = default);
    Task<NotificationResponse> CreateAsync(Guid userId, string title, string message, string type, CancellationToken cancellationToken = default);

    // Default bodies keep older test doubles/source-compatible while the real NotificationService
    // provides the complete implementation used by the API.
    Task<int> MarkAllReadAsync(Guid userId, CancellationToken cancellationToken = default)
        => throw new NotSupportedException();

    Task DeleteAsync(Guid userId, Guid notificationId, CancellationToken cancellationToken = default)
        => throw new NotSupportedException();

    Task<int> ClearAsync(Guid userId, CancellationToken cancellationToken = default)
        => throw new NotSupportedException();
}
