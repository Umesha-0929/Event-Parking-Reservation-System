using SEVPMS.Domain.Entities.Events;

namespace SEVPMS.Application.Interfaces.Repositories;

public interface IEventCategoryRepository
{
    Task<IReadOnlyList<EventCategory>> GetAsync(bool includeInactive, CancellationToken cancellationToken = default);
    Task<EventCategory?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<EventCategory?> FindActiveAsync(string nameOrCode, CancellationToken cancellationToken = default);
    Task AddAsync(EventCategory category, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
