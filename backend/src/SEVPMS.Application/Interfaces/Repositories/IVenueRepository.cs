using SEVPMS.Domain.Entities.Venues;

namespace SEVPMS.Application.Interfaces.Repositories;

public interface IVenueRepository
{
    Task<IReadOnlyList<Venue>> GetAllAsync(
        CancellationToken cancellationToken = default);

    async Task<IReadOnlyList<Venue>> GetPageAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var all = await GetAllAsync(cancellationToken);
        var safePage = Math.Max(page, 1);
        var safePageSize = Math.Clamp(pageSize, 1, 200);
        return all.Skip((safePage - 1) * safePageSize).Take(safePageSize).ToArray();
    }

    Task<IReadOnlyList<Venue>> GetByOwnerUserIdAsync(
        Guid ownerUserId,
        CancellationToken cancellationToken = default);

    async Task<IReadOnlyList<Venue>> GetByOwnerPageAsync(
        Guid ownerUserId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var all = await GetByOwnerUserIdAsync(ownerUserId, cancellationToken);
        var safePage = Math.Max(page, 1);
        var safePageSize = Math.Clamp(pageSize, 1, 200);
        return all.Skip((safePage - 1) * safePageSize).Take(safePageSize).ToArray();
    }

    Task<Venue?> GetByIdAsync(
        Guid venueId,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        Venue venue,
        CancellationToken cancellationToken = default);

    Task<bool> IsReferencedAsync(Guid venueId, CancellationToken cancellationToken = default);
    Task DeleteAsync(Venue venue, CancellationToken cancellationToken = default);

    Task SaveChangesAsync(
        CancellationToken cancellationToken = default);
}