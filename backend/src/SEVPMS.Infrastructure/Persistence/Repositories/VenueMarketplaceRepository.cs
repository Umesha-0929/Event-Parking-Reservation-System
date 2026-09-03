using Microsoft.EntityFrameworkCore;
using SEVPMS.Application.Interfaces.Repositories;
using SEVPMS.Domain.Entities.Venues;
using SEVPMS.Domain.Enums;

namespace SEVPMS.Infrastructure.Persistence.Repositories;

public sealed class VenueMarketplaceRepository(SEVPMSDbContext dbContext) : IVenueMarketplaceRepository
{
    public async Task<IReadOnlyList<VenueFacility>> GetFacilitiesAsync(
        bool includeInactive,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.Set<VenueFacility>().AsNoTracking();
        if (!includeInactive)
            query = query.Where(x => x.IsActive);

        return await query
            .OrderBy(x => x.Category)
            .ThenBy(x => x.Name)
            .ToListAsync(cancellationToken);
    }

    public Task<VenueFacility?> GetFacilityAsync(Guid id, CancellationToken cancellationToken = default)
        => dbContext.Set<VenueFacility>().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task AddFacilityAsync(VenueFacility facility, CancellationToken cancellationToken = default)
        => await dbContext.Set<VenueFacility>().AddAsync(facility, cancellationToken);

    public async Task<IReadOnlyList<VenueFacilityLink>> GetFacilityLinksAsync(
        Guid venueId,
        CancellationToken cancellationToken = default)
        => await dbContext.Set<VenueFacilityLink>()
            .AsNoTracking()
            .Where(x => x.VenueId == venueId)
            .ToListAsync(cancellationToken);

    public async Task ReplaceFacilityLinksAsync(
        Guid venueId,
        IReadOnlyCollection<Guid> facilityIds,
        CancellationToken cancellationToken = default)
    {
        var existing = await dbContext.Set<VenueFacilityLink>()
            .Where(x => x.VenueId == venueId)
            .ToListAsync(cancellationToken);

        dbContext.RemoveRange(existing);

        foreach (var id in facilityIds.Distinct())
        {
            dbContext.Set<VenueFacilityLink>().Add(
                new VenueFacilityLink
                {
                    VenueId = venueId,
                    FacilityId = id
                });
        }
    }

    public async Task<IReadOnlyList<VenueMedia>> GetMediaAsync(
        Guid venueId,
        CancellationToken cancellationToken = default)
        => await dbContext.Set<VenueMedia>()
            .AsNoTracking()
            .Where(x => x.VenueId == venueId)
            .OrderBy(x => x.SortOrder)
            .ToListAsync(cancellationToken);

    public async Task AddMediaAsync(VenueMedia media, CancellationToken cancellationToken = default)
        => await dbContext.Set<VenueMedia>().AddAsync(media, cancellationToken);

    public async Task<IReadOnlyList<VenueRate>> GetRatesAsync(
        Guid venueId,
        CancellationToken cancellationToken = default)
        => await dbContext.Set<VenueRate>()
            .AsNoTracking()
            .Where(x => x.VenueId == venueId && x.IsActive)
            .OrderBy(x => x.Amount)
            .ToListAsync(cancellationToken);

    public async Task AddRateAsync(VenueRate rate, CancellationToken cancellationToken = default)
        => await dbContext.Set<VenueRate>().AddAsync(rate, cancellationToken);

    public async Task<IReadOnlyList<VenueAvailability>> GetAvailabilityAsync(
        Guid venueId,
        CancellationToken cancellationToken = default)
        => await dbContext.Set<VenueAvailability>()
            .AsNoTracking()
            .Where(x => x.VenueId == venueId)
            .OrderBy(x => x.StartAtUtc)
            .ToListAsync(cancellationToken);

    public Task<bool> HasBlockingAvailabilityAsync(
        Guid venueId,
        DateTime startAtUtc,
        DateTime endAtUtc,
        CancellationToken cancellationToken = default)
        => dbContext.Set<VenueAvailability>()
            .AsNoTracking()
            .AnyAsync(
                x => x.VenueId == venueId &&
                     x.Type != VenueAvailabilityType.Available &&
                     x.StartAtUtc < endAtUtc &&
                     x.EndAtUtc > startAtUtc,
                cancellationToken);

    public async Task AddAvailabilityAsync(
        VenueAvailability availability,
        CancellationToken cancellationToken = default)
        => await dbContext.Set<VenueAvailability>().AddAsync(availability, cancellationToken);

    public async Task<IReadOnlyList<VenueLayoutTemplate>> GetLayoutTemplatesAsync(
        Guid venueId,
        CancellationToken cancellationToken = default)
        => await dbContext.Set<VenueLayoutTemplate>()
            .AsNoTracking()
            .Where(x => x.VenueId == venueId && x.IsActive)
            .OrderBy(x => x.Name)
            .ThenByDescending(x => x.Version)
            .ToListAsync(cancellationToken);

    public async Task AddLayoutTemplateAsync(
        VenueLayoutTemplate template,
        CancellationToken cancellationToken = default)
        => await dbContext.Set<VenueLayoutTemplate>().AddAsync(template, cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        => dbContext.SaveChangesAsync(cancellationToken);
}
