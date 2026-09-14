using Microsoft.EntityFrameworkCore;
using SEVPMS.Application.Features.Parking.Interfaces;
using SEVPMS.Domain.Entities.Parking;

namespace SEVPMS.Infrastructure.Persistence.Repositories;

public sealed class ParkingRouteRepository(
    SEVPMSDbContext dbContext) : IParkingRouteRepository
{
    public async Task<IReadOnlyList<ParkingNode>> GetNodesByVenueAsync(
        Guid venueId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext
            .Set<ParkingNode>()
            .Where(node => node.VenueId == venueId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ParkingEdge>> GetEdgesByVenueAsync(
        Guid venueId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext
            .Set<ParkingEdge>()
            .Where(edge => edge.VenueId == venueId)
            .ToListAsync(cancellationToken);
    }
}