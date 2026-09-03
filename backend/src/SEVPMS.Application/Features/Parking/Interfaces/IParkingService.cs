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

    Task<ParkingZoneDto> CreateZoneAsync(
        UpsertParkingZoneRequest request,
        CancellationToken cancellationToken = default);

    Task<ParkingZoneDto> UpdateZoneAsync(
        Guid parkingZoneId,
        UpsertParkingZoneRequest request,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteZoneAsync(
        Guid parkingZoneId,
        CancellationToken cancellationToken = default);

    Task<ParkingSlotDto> CreateSlotAsync(
        UpsertParkingSlotRequest request,
        CancellationToken cancellationToken = default);

    Task<ParkingSlotDto> UpdateSlotAsync(
        Guid parkingSlotId,
        UpsertParkingSlotRequest request,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteSlotAsync(
        Guid parkingSlotId,
        CancellationToken cancellationToken = default);
}