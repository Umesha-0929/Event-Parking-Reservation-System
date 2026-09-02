using SEVPMS.Domain.Entities.Venues;

namespace SEVPMS.Application.Interfaces.Repositories;

public interface IVenueRepository
{
    Task<IReadOnlyList<Venue>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Venue>> GetByOwnerUserIdAsync(
        Guid ownerUserId,
        CancellationToken cancellationToken = default);

    Task<Venue?> GetByIdAsync(
        Guid venueId,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        Venue venue,
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(
        CancellationToken cancellationToken = default);
}