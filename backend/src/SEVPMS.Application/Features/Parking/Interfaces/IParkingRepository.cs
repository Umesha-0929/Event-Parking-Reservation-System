using SEVPMS.Domain.Entities.Parking;

namespace SEVPMS.Application.Features.Parking.Interfaces;

public interface IParkingRepository
{
    Task<IReadOnlyList<ParkingZone>> GetZonesByVenueAsync(
        Guid venueId,
        CancellationToken cancellationToken = default);

    async Task<IReadOnlyList<ParkingZone>> GetZonesByVenuePageAsync(
        Guid venueId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var all = await GetZonesByVenueAsync(venueId, cancellationToken);
        var safePage = Math.Max(page, 1);
        var safePageSize = Math.Clamp(pageSize, 1, 200);
        return all.Skip((safePage - 1) * safePageSize).Take(safePageSize).ToArray();
    }

    Task<IReadOnlyList<ParkingSlot>> GetSlotsByZoneAsync(
        Guid parkingZoneId,
        CancellationToken cancellationToken = default);

    async Task<IReadOnlyList<ParkingSlot>> GetSlotsByZonePageAsync(
        Guid parkingZoneId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var all = await GetSlotsByZoneAsync(parkingZoneId, cancellationToken);
        var safePage = Math.Max(page, 1);
        var safePageSize = Math.Clamp(pageSize, 1, 200);
        return all.Skip((safePage - 1) * safePageSize).Take(safePageSize).ToArray();
    }

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