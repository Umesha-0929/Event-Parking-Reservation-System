using SEVPMS.Application.Features.Parking.DTOs;
using SEVPMS.Application.Features.Parking.Interfaces;
using SEVPMS.Domain.Entities.Parking;

namespace SEVPMS.Application.Features.Parking.Services;

public sealed class ParkingService(
    IParkingRepository repository) : IParkingService
{
    public async Task<IReadOnlyList<ParkingZoneDto>> GetZonesByVenueAsync(
        Guid venueId,
        CancellationToken cancellationToken = default)
    {
        var zones = await repository.GetZonesByVenueAsync(
            venueId,
            cancellationToken);

        return zones
            .Select(ToZoneDto)
            .ToList();
    }

    public async Task<IReadOnlyList<ParkingSlotDto>> GetSlotsByZoneAsync(
        Guid parkingZoneId,
        CancellationToken cancellationToken = default)
    {
        var slots = await repository.GetSlotsByZoneAsync(
            parkingZoneId,
            cancellationToken);

        return slots
            .Select(ToSlotDto)
            .ToList();
    }

    public async Task<ParkingSlotDto?> GetSlotByIdAsync(
        Guid parkingSlotId,
        CancellationToken cancellationToken = default)
    {
        var slot = await repository.GetSlotByIdAsync(
            parkingSlotId,
            cancellationToken);

        return slot is null
            ? null
            : ToSlotDto(slot);
    }

    private static ParkingZoneDto ToZoneDto(
        ParkingZone zone)
    {
        return new ParkingZoneDto
        {
            Id = zone.Id,
            VenueId = zone.VenueId,
            EventId = zone.EventId,
            Name = zone.Name,
            Level = zone.Level,
            EntranceName = zone.EntranceName
        };
    }

    private static ParkingSlotDto ToSlotDto(
        ParkingSlot slot)
    {
        return new ParkingSlotDto
        {
            Id = slot.Id,
            ParkingZoneId = slot.ParkingZoneId,
            EventId = slot.EventId,
            SlotCode = slot.SlotCode,
            X = slot.X,
            Y = slot.Y,
            IsAccessible = slot.IsAccessible,
            Status = slot.Status
        };
    }
}