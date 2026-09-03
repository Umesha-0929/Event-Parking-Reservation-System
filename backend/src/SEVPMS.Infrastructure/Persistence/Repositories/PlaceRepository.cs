using Microsoft.EntityFrameworkCore;
using SEVPMS.Application.Features.Places.Interfaces;
using SEVPMS.Domain.Entities.Places;

namespace SEVPMS.Infrastructure.Persistence.Repositories;

public sealed class PlaceRepository(SEVPMSDbContext dbContext)
    : IPlaceRepository
{
    public async Task<IReadOnlyList<NearbyPlace>> GetByVenueAsync(
        Guid venueId,
        CancellationToken cancellationToken = default)
        => await dbContext.Set<NearbyPlace>()
            .AsNoTracking()
            .Where(place => place.VenueId == venueId)
            .ToListAsync(cancellationToken);

    public Task<NearbyPlace?> GetByIdAsync(
        Guid placeId,
        CancellationToken cancellationToken = default)
        => dbContext.Set<NearbyPlace>()
            .SingleOrDefaultAsync(place => place.Id == placeId, cancellationToken);

    public async Task AddAsync(
        NearbyPlace place,
        CancellationToken cancellationToken = default)
        => await dbContext.Set<NearbyPlace>().AddAsync(place, cancellationToken);

    public void Update(NearbyPlace place)
        => dbContext.Set<NearbyPlace>().Update(place);

    public void Remove(NearbyPlace place)
        => dbContext.Set<NearbyPlace>().Remove(place);

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        => await dbContext.SaveChangesAsync(cancellationToken);
}
