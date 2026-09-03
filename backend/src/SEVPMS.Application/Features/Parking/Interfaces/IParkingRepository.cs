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

    Task<ParkingZone?> GetZoneByIdAsync(
        Guid parkingZoneId,
        CancellationToken cancellationToken = default);

    Task AddZoneAsync(
        ParkingZone zone,
        CancellationToken cancellationToken = default);

    Task AddSlotAsync(
        ParkingSlot slot,
        CancellationToken cancellationToken = default);

    void UpdateZone(
        ParkingZone zone);

    void UpdateSlot(
        ParkingSlot slot);

    void DeleteZone(
        ParkingZone zone);

    void DeleteSlot(
        ParkingSlot slot);

    Task SaveChangesAsync(
        CancellationToken cancellationToken = default);
}