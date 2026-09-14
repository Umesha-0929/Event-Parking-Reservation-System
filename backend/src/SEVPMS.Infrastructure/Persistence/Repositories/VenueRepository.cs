using Microsoft.EntityFrameworkCore;
using SEVPMS.Application.Common.Paging;
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

    public async Task<IReadOnlyList<Venue>> GetPageAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var (_, size, skip) = PagingRules.Normalize(page, pageSize);
        return await dbContext.Venues
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedAtUtc)
            .Skip(skip)
            .Take(size)
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

    public async Task<IReadOnlyList<Venue>> GetByOwnerPageAsync(
        Guid ownerUserId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var (_, size, skip) = PagingRules.Normalize(page, pageSize);
        return await dbContext.Venues
            .AsNoTracking()
            .Where(x => x.OwnerUserId == ownerUserId)
            .OrderByDescending(x => x.CreatedAtUtc)
            .Skip(skip)
            .Take(size)
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

    public async Task<bool> IsReferencedAsync(Guid venueId, CancellationToken cancellationToken = default)
    {
        if (await dbContext.Set<SEVPMS.Domain.Entities.Events.Event>().AsNoTracking().AnyAsync(x => x.VenueId == venueId, cancellationToken)) return true;
        if (await dbContext.Set<SEVPMS.Domain.Entities.VenueRentals.VenueRentalRequest>().AsNoTracking().AnyAsync(x => x.VenueId == venueId, cancellationToken)) return true;
        return false;
    }

    public Task DeleteAsync(Venue venue, CancellationToken cancellationToken = default)
    {
        dbContext.Venues.Remove(venue);
        return Task.CompletedTask;
    }


    public async Task SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        await dbContext.SaveChangesAsync(
            cancellationToken);
    }
}