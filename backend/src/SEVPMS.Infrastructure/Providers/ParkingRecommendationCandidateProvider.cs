using SEVPMS.Application.Features.Parking.DTOs;
using SEVPMS.Application.Features.Parking.Interfaces;
using SEVPMS.Application.Features.Vehicles.Interfaces;

namespace SEVPMS.Infrastructure.Persistence.Providers;

public sealed class ParkingRecommendationCandidateProvider(
    IParkingRepository parkingRepository,
    IParkingRouteRepository routeRepository,
    ISavedVehicleRepository vehicleRepository)
    : IParkingRecommendationCandidateProvider
{
    public async Task<IReadOnlyList<ParkingRecommendationCandidateDto>> GetCandidatesAsync(
        ParkingRecommendationRequest request,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var nodes = await routeRepository.GetNodesByVenueAsync(
            request.VenueId,
            cancellationToken);

        var entranceNode = nodes
            .SingleOrDefault(node => node.Id == request.EntranceNodeId);

        if (entranceNode is null)
        {
            return [];
        }

        var vehicleSuitable = true;

        if (request.SavedVehicleId.HasValue)
        {
            var vehicle = await vehicleRepository.GetByIdAsync(
                request.SavedVehicleId.Value,
                cancellationToken);

            if (vehicle is null || vehicle.UserId != userId)
            {
                return [];
            }

            vehicleSuitable = true;
        }

        var zones = await parkingRepository.GetZonesByVenueAsync(
            request.VenueId,
            cancellationToken);

        var candidates =
            new List<ParkingRecommendationCandidateDto>();

        foreach (var zone in zones)
        {
            if (request.EventId.HasValue &&
                zone.EventId.HasValue &&
                zone.EventId != request.EventId)
            {
                continue;
            }

            var slots = await parkingRepository.GetSlotsByZoneAsync(
                zone.Id,
                cancellationToken);

            foreach (var slot in slots)
            {
                if (request.EventId.HasValue &&
                    slot.EventId.HasValue &&
                    slot.EventId != request.EventId)
                {
                    continue;
                }

                var deltaX = slot.X - entranceNode.X;
                var deltaY = slot.Y - entranceNode.Y;

                var distanceSquared =
                    (deltaX * deltaX) +
                    (deltaY * deltaY);

                var distanceCost =
                    (decimal)Math.Sqrt((double)distanceSquared);

                candidates.Add(
                    new ParkingRecommendationCandidateDto
                    {
                        ParkingSlotId = slot.Id,
                        ParkingZoneId = slot.ParkingZoneId,
                        SlotCode = slot.SlotCode,
                        DistanceCost = distanceCost,
                        IsAvailable = string.Equals(
                            slot.Status,
                            "Available",
                            StringComparison.OrdinalIgnoreCase),
                        IsAccessible = slot.IsAccessible,
                        IsVehicleSuitable = vehicleSuitable
                    });
            }
        }

        return candidates;
    }
}