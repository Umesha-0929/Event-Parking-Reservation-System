using Microsoft.EntityFrameworkCore;
using SEVPMS.Application.Interfaces.Repositories;
using SEVPMS.Domain.Entities.Events;
using SEVPMS.Domain.Enums;

namespace SEVPMS.Infrastructure.Persistence.Repositories;

public sealed class EventRepository(SEVPMSDbContext dbContext) : IEventRepository
{
    public async Task<IReadOnlyList<Event>> GetPublishedAsync(CancellationToken cancellationToken = default)
        => await dbContext.Set<Event>()
            .AsNoTracking()
            .Where(x => x.Status == EventStatus.Published)
            .OrderBy(x => x.StartAtUtc)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Event>> GetByOrganizerUserIdAsync(Guid organizerUserId, CancellationToken cancellationToken = default)
        => await dbContext.Set<Event>()
            .AsNoTracking()
            .Where(x => x.OrganizerUserId == organizerUserId)
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToListAsync(cancellationToken);

    public Task<Event?> GetByIdAsync(Guid eventId, CancellationToken cancellationToken = default)
        => dbContext.Set<Event>().FirstOrDefaultAsync(x => x.Id == eventId, cancellationToken);

    public async Task AddAsync(Event eventEntity, CancellationToken cancellationToken = default)
        => await dbContext.Set<Event>().AddAsync(eventEntity, cancellationToken);

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        => await dbContext.SaveChangesAsync(cancellationToken);
}
