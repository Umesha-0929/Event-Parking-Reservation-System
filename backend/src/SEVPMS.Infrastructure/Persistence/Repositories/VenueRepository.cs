using Microsoft.EntityFrameworkCore;
using SEVPMS.Application.Interfaces.Repositories;
using SEVPMS.Domain.Entities.Venues;

namespace SEVPMS.Infrastructure.Persistence.Repositories;

public sealed class VenueRepository(
    SEVPMSDbContext dbContext)
    : IVenueRepository
{
    public async Task<IReadOnlyList<Venue>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Venues
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Venue>> GetByOwnerUserIdAsync(
        Guid ownerUserId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Venues
            .AsNoTracking()
            .Where(x => x.OwnerUserId == ownerUserId)
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public Task<Venue?> GetByIdAsync(
        Guid venueId,
        CancellationToken cancellationToken = default)
    {
        return dbContext.Venues
            .FirstOrDefaultAsync(
                x => x.Id == venueId,
                cancellationToken);
    }

    public async Task AddAsync(
        Venue venue,
        CancellationToken cancellationToken = default)
    {
        await dbContext.Venues.AddAsync(
            venue,
            cancellationToken);
    }

    public async Task SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        await dbContext.SaveChangesAsync(
            cancellationToken);
    }
}