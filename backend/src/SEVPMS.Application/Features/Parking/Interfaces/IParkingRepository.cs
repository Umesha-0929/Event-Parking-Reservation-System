using SEVPMS.Domain.Entities.Parking;

namespace SEVPMS.Application.Features.Parking.Interfaces;

public interface IParkingRepository
{
    Task<IReadOnlyList<ParkingZone>> GetZonesByVenueAsync(
        Guid venueId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ParkingSlot>> GetSlotsByZoneAsync(
        Guid parkingZoneId,
        CancellationToken cancellationToken = default);

    Task<ParkingSlot?> GetSlotByIdAsync(
        Guid parkingSlotId,
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(
        CancellationToken cancellationToken = default);
}