using SEVPMS.Domain.Entities.Events;

namespace SEVPMS.Application.Interfaces.Repositories;

public interface IEventRepository
{
    Task<IReadOnlyList<Event>> GetPublishedAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Event>> GetByOrganizerUserIdAsync(Guid organizerUserId, CancellationToken cancellationToken = default);
    Task<Event?> GetByIdAsync(Guid eventId, CancellationToken cancellationToken = default);
    Task AddAsync(Event eventEntity, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
