using SEVPMS.Application.Features.Parking.DTOs;

namespace SEVPMS.Application.Features.Parking.Interfaces;

public interface IParkingService
{
    Task<IReadOnlyList<ParkingZoneDto>> GetZonesByVenueAsync(
        Guid venueId,
        CancellationToken cancellationToken = default);

    async Task<IReadOnlyList<ParkingZoneDto>> GetZonesByVenuePageAsync(
        Guid venueId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var all = await GetZonesByVenueAsync(venueId, cancellationToken);
        var safePage = Math.Max(page, 1);
        var safePageSize = Math.Clamp(pageSize, 1, 200);
        return all.Skip((safePage - 1) * safePageSize).Take(safePageSize).ToArray();
    }

    Task<IReadOnlyList<ParkingSlotDto>> GetSlotsByZoneAsync(
        Guid parkingZoneId,
        CancellationToken cancellationToken = default);

    async Task<IReadOnlyList<ParkingSlotDto>> GetSlotsByZonePageAsync(
        Guid parkingZoneId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var all = await GetSlotsByZoneAsync(parkingZoneId, cancellationToken);
        var safePage = Math.Max(page, 1);
        var safePageSize = Math.Clamp(pageSize, 1, 200);
        return all.Skip((safePage - 1) * safePageSize).Take(safePageSize).ToArray();
    }

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

    Task<IReadOnlyList<ParkingSlotDto>> CreateSlotsBulkAsync(
        IReadOnlyCollection<UpsertParkingSlotRequest> requests,
        CancellationToken cancellationToken = default);

    Task<ParkingSlotDto> UpdateSlotAsync(
        Guid parkingSlotId,
        UpsertParkingSlotRequest request,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteSlotAsync(
        Guid parkingSlotId,
        CancellationToken cancellationToken = default);
}