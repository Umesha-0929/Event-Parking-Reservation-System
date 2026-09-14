using SEVPMS.Domain.Entities.Notifications;

namespace SEVPMS.Application.Interfaces.Repositories;

public interface INotificationRepository
{
    Task<Notification?> GetByIdAsync(Guid notificationId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Notification>> GetByUserAsync(Guid userId, CancellationToken cancellationToken = default);
    async Task<IReadOnlyList<Notification>> GetByUserPageAsync(
        Guid userId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var all = await GetByUserAsync(userId, cancellationToken);
        var safePage = Math.Max(page, 1);
        var safePageSize = Math.Clamp(pageSize, 1, 200);
        return all.Skip((safePage - 1) * safePageSize).Take(safePageSize).ToArray();
    }
    Task AddAsync(Notification notification, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);

    // Default bodies preserve compatibility with any existing repository fakes in tests.
    Task<IReadOnlyList<Notification>> GetTrackedByUserAsync(Guid userId, CancellationToken cancellationToken = default)
        => throw new NotSupportedException();

    void Remove(Notification notification) => throw new NotSupportedException();
    void RemoveRange(IEnumerable<Notification> notifications) => throw new NotSupportedException();
}
