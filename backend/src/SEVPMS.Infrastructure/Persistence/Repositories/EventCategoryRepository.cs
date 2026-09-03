using Microsoft.EntityFrameworkCore;
using SEVPMS.Application.Interfaces.Repositories;
using SEVPMS.Domain.Entities.Events;

namespace SEVPMS.Infrastructure.Persistence.Repositories;

public sealed class EventCategoryRepository(SEVPMSDbContext dbContext) : IEventCategoryRepository
{
    public async Task<IReadOnlyList<EventCategory>> GetAsync(bool includeInactive, CancellationToken cancellationToken = default)
    {
        var query = dbContext.Set<EventCategory>().AsNoTracking();
        if (!includeInactive)
            query = query.Where(x => x.IsActive);

        return await query.OrderBy(x => x.Name).ToListAsync(cancellationToken);
    }

    public Task<EventCategory?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => dbContext.Set<EventCategory>().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task<EventCategory?> FindActiveAsync(string nameOrCode, CancellationToken cancellationToken = default)
    {
        var value = nameOrCode.Trim();
        return dbContext.Set<EventCategory>().AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.IsActive && (x.Name == value || x.Code == value),
                cancellationToken);
    }

    public async Task AddAsync(EventCategory category, CancellationToken cancellationToken = default)
        => await dbContext.Set<EventCategory>().AddAsync(category, cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        => dbContext.SaveChangesAsync(cancellationToken);
}
