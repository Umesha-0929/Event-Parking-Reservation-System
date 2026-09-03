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

    public async Task<ParkingZoneDto> CreateZoneAsync(
        UpsertParkingZoneRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.VenueId == Guid.Empty)
        {
            throw new ArgumentException(
                "Venue is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            throw new ArgumentException(
                "Parking zone name is required.");
        }

        var zone = new ParkingZone
        {
            VenueId = request.VenueId,
            EventId = request.EventId,
            Name = request.Name.Trim(),
            Level = request.Level?.Trim() ?? string.Empty,
            EntranceName = request.EntranceName?.Trim() ?? string.Empty
        };

        await repository.AddZoneAsync(
            zone,
            cancellationToken);

        await repository.SaveChangesAsync(
            cancellationToken);

        return ToZoneDto(zone);
    }

    public async Task<ParkingZoneDto> UpdateZoneAsync(
        Guid parkingZoneId,
        UpsertParkingZoneRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (parkingZoneId == Guid.Empty)
        {
            throw new ArgumentException(
                "Parking zone is required.");
        }

        if (request.VenueId == Guid.Empty)
        {
            throw new ArgumentException(
                "Venue is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            throw new ArgumentException(
                "Parking zone name is required.");
        }

        var zone =
            await repository.GetZoneByIdAsync(
                parkingZoneId,
                cancellationToken)
            ?? throw new KeyNotFoundException(
                "Parking zone was not found.");

        zone.VenueId = request.VenueId;
        zone.EventId = request.EventId;
        zone.Name = request.Name.Trim();
        zone.Level = request.Level?.Trim() ?? string.Empty;
        zone.EntranceName =
            request.EntranceName?.Trim() ?? string.Empty;
        zone.UpdatedAtUtc = DateTime.UtcNow;

        repository.UpdateZone(zone);

        await repository.SaveChangesAsync(
            cancellationToken);

        return ToZoneDto(zone);
    }

    public async Task<bool> DeleteZoneAsync(
        Guid parkingZoneId,
        CancellationToken cancellationToken = default)
    {
        if (parkingZoneId == Guid.Empty)
        {
            throw new ArgumentException(
                "Parking zone is required.");
        }

        var zone =
            await repository.GetZoneByIdAsync(
                parkingZoneId,
                cancellationToken);

        if (zone is null)
        {
            return false;
        }

        var slots =
            await repository.GetSlotsByZoneAsync(
                parkingZoneId,
                cancellationToken);

        foreach (var slot in slots)
        {
            repository.DeleteSlot(slot);
        }

        repository.DeleteZone(zone);

        await repository.SaveChangesAsync(
            cancellationToken);

        return true;
    }

    public async Task<ParkingSlotDto> CreateSlotAsync(
        UpsertParkingSlotRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        ValidateSlotRequest(request);

        var zone =
            await repository.GetZoneByIdAsync(
                request.ParkingZoneId,
                cancellationToken)
            ?? throw new KeyNotFoundException(
                "Parking zone was not found.");

        if (request.EventId.HasValue &&
            zone.EventId.HasValue &&
            request.EventId.Value != zone.EventId.Value)
        {
            throw new ArgumentException(
                "Parking slot event does not match the parking zone event.");
        }

        var existingSlots =
            await repository.GetSlotsByZoneAsync(
                request.ParkingZoneId,
                cancellationToken);

        if (existingSlots.Any(
                slot => string.Equals(
                    slot.SlotCode,
                    request.SlotCode.Trim(),
                    StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException(
                "A parking slot with this code already exists in the selected zone.");
        }

        var slot = new ParkingSlot
        {
            ParkingZoneId = request.ParkingZoneId,
            EventId = request.EventId,
            SlotCode = request.SlotCode.Trim(),
            X = request.X,
            Y = request.Y,
            IsAccessible = request.IsAccessible,
            Status = NormalizeStatus(request.Status)
        };

        await repository.AddSlotAsync(
            slot,
            cancellationToken);

        await repository.SaveChangesAsync(
            cancellationToken);

        return ToSlotDto(slot);
    }

    public async Task<ParkingSlotDto> UpdateSlotAsync(
        Guid parkingSlotId,
        UpsertParkingSlotRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (parkingSlotId == Guid.Empty)
        {
            throw new ArgumentException(
                "Parking slot is required.");
        }

        ValidateSlotRequest(request);

        var slot =
            await repository.GetSlotByIdAsync(
                parkingSlotId,
                cancellationToken)
            ?? throw new KeyNotFoundException(
                "Parking slot was not found.");

        var zone =
            await repository.GetZoneByIdAsync(
                request.ParkingZoneId,
                cancellationToken)
            ?? throw new KeyNotFoundException(
                "Parking zone was not found.");

        if (request.EventId.HasValue &&
            zone.EventId.HasValue &&
            request.EventId.Value != zone.EventId.Value)
        {
            throw new ArgumentException(
                "Parking slot event does not match the parking zone event.");
        }

        var existingSlots =
            await repository.GetSlotsByZoneAsync(
                request.ParkingZoneId,
                cancellationToken);

        if (existingSlots.Any(
                existing =>
                    existing.Id != parkingSlotId &&
                    string.Equals(
                        existing.SlotCode,
                        request.SlotCode.Trim(),
                        StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException(
                "A parking slot with this code already exists in the selected zone.");
        }

        slot.ParkingZoneId = request.ParkingZoneId;
        slot.EventId = request.EventId;
        slot.SlotCode = request.SlotCode.Trim();
        slot.X = request.X;
        slot.Y = request.Y;
        slot.IsAccessible = request.IsAccessible;
        slot.Status = NormalizeStatus(request.Status);
        slot.UpdatedAtUtc = DateTime.UtcNow;

        repository.UpdateSlot(slot);

        await repository.SaveChangesAsync(
            cancellationToken);

        return ToSlotDto(slot);
    }

    public async Task<bool> DeleteSlotAsync(
        Guid parkingSlotId,
        CancellationToken cancellationToken = default)
    {
        if (parkingSlotId == Guid.Empty)
        {
            throw new ArgumentException(
                "Parking slot is required.");
        }

        var slot =
            await repository.GetSlotByIdAsync(
                parkingSlotId,
                cancellationToken);

        if (slot is null)
        {
            return false;
        }

        repository.DeleteSlot(slot);

        await repository.SaveChangesAsync(
            cancellationToken);

        return true;
    }

    private static void ValidateSlotRequest(
        UpsertParkingSlotRequest request)
    {
        if (request.ParkingZoneId == Guid.Empty)
        {
            throw new ArgumentException(
                "Parking zone is required.");
        }

        if (string.IsNullOrWhiteSpace(
                request.SlotCode))
        {
            throw new ArgumentException(
                "Parking slot code is required.");
        }
    }

    private static string NormalizeStatus(
        string? status)
    {
        var value =
            status?.Trim()
            ?? string.Empty;

        if (string.IsNullOrWhiteSpace(value))
        {
            return "Available";
        }

        if (value.Equals(
                "Available",
                StringComparison.OrdinalIgnoreCase))
        {
            return "Available";
        }

        if (value.Equals(
                "Reserved",
                StringComparison.OrdinalIgnoreCase))
        {
            return "Reserved";
        }

        if (value.Equals(
                "Occupied",
                StringComparison.OrdinalIgnoreCase))
        {
            return "Occupied";
        }

        if (value.Equals(
                "Blocked",
                StringComparison.OrdinalIgnoreCase))
        {
            return "Blocked";
        }

        throw new ArgumentException(
            "Parking slot status must be Available, Reserved, Occupied, or Blocked.");
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