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

    public async Task<ParkingZone?> GetZoneByIdAsync(
        Guid parkingZoneId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext
            .Set<ParkingZone>()
            .SingleOrDefaultAsync(
                zone => zone.Id == parkingZoneId,
                cancellationToken);
    }

    public async Task AddZoneAsync(
        ParkingZone zone,
        CancellationToken cancellationToken = default)
    {
        await dbContext
            .Set<ParkingZone>()
            .AddAsync(
                zone,
                cancellationToken);
    }

    public async Task AddSlotAsync(
        ParkingSlot slot,
        CancellationToken cancellationToken = default)
    {
        await dbContext
            .Set<ParkingSlot>()
            .AddAsync(
                slot,
                cancellationToken);
    }

    public void UpdateZone(
        ParkingZone zone)
    {
        dbContext
            .Set<ParkingZone>()
            .Update(zone);
    }

    public void UpdateSlot(
        ParkingSlot slot)
    {
        dbContext
            .Set<ParkingSlot>()
            .Update(slot);
    }

    public void DeleteZone(
        ParkingZone zone)
    {
        dbContext
            .Set<ParkingZone>()
            .Remove(zone);
    }

    public void DeleteSlot(
        ParkingSlot slot)
    {
        dbContext
            .Set<ParkingSlot>()
            .Remove(slot);
    }

    public async Task SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        await dbContext.SaveChangesAsync(
            cancellationToken);
    }
}