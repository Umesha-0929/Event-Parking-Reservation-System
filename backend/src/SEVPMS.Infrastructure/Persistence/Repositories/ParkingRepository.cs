using Microsoft.EntityFrameworkCore;
using SEVPMS.Application.Features.Parking.Interfaces;
using SEVPMS.Domain.Entities.Parking;

namespace SEVPMS.Infrastructure.Persistence.Repositories;

public sealed class ParkingRepository(
    SEVPMSDbContext dbContext) : IParkingRepository
{
    public async Task<IReadOnlyList<ParkingZone>> GetZonesByVenueAsync(
        Guid venueId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext
            .Set<ParkingZone>()
            .Where(zone => zone.VenueId == venueId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ParkingSlot>> GetSlotsByZoneAsync(
        Guid parkingZoneId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext
            .Set<ParkingSlot>()
            .Where(slot => slot.ParkingZoneId == parkingZoneId)
            .ToListAsync(cancellationToken);
    }

    public async Task<ParkingSlot?> GetSlotByIdAsync(
        Guid parkingSlotId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext
            .Set<ParkingSlot>()
            .SingleOrDefaultAsync(
                slot => slot.Id == parkingSlotId,
                cancellationToken);
    }

    public async Task SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}