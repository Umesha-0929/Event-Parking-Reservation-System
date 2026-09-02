using Microsoft.EntityFrameworkCore;
using SEVPMS.Application.Interfaces.Repositories;
using SEVPMS.Domain.Entities.Notifications;

namespace SEVPMS.Infrastructure.Persistence.Repositories;

public sealed class NotificationRepository(SEVPMSDbContext dbContext) : INotificationRepository
{
    public Task<Notification?> GetByIdAsync(Guid notificationId, CancellationToken cancellationToken = default)
        => dbContext.Set<Notification>().FirstOrDefaultAsync(x => x.Id == notificationId, cancellationToken);

    public async Task<IReadOnlyList<Notification>> GetByUserAsync(Guid userId, CancellationToken cancellationToken = default)
        => await dbContext.Set<Notification>()
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(Notification notification, CancellationToken cancellationToken = default)
        => await dbContext.Set<Notification>().AddAsync(notification, cancellationToken);

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        => await dbContext.SaveChangesAsync(cancellationToken);
}
