using SEVPMS.Application.Features.Parking.DTOs;

namespace SEVPMS.Application.Features.Parking.Interfaces;

public interface IParkingService
{
    Task<IReadOnlyList<ParkingZoneDto>> GetZonesByVenueAsync(
        Guid venueId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ParkingSlotDto>> GetSlotsByZoneAsync(
        Guid parkingZoneId,
        CancellationToken cancellationToken = default);

    Task<ParkingSlotDto?> GetSlotByIdAsync(
        Guid parkingSlotId,
        CancellationToken cancellationToken = default);
}