using SEVPMS.Application.Features.Parking.DTOs;
using SEVPMS.Application.Features.Parking.Interfaces;

namespace SEVPMS.Application.Features.Parking.Services;

public sealed class ParkingRecommendationService
    : IParkingRecommendationService
{
    public ParkingRecommendationDto? RecommendBestSlot(
        IReadOnlyList<ParkingRecommendationCandidateDto> candidates,
        bool requiresAccessibleParking)
    {
        var eligibleCandidates = candidates
            .Where(candidate => candidate.IsAvailable)
            .Where(candidate => candidate.IsVehicleSuitable)
            .Where(candidate =>
                !requiresAccessibleParking ||
                candidate.IsAccessible)
            .OrderBy(candidate => candidate.DistanceCost)
            .ThenBy(candidate => candidate.SlotCode)
            .ToList();

        var best = eligibleCandidates.FirstOrDefault();

        if (best is null)
        {
            return null;
        }

        var reason = requiresAccessibleParking
            ? "Nearest available accessible slot suitable for the selected vehicle."
            : "Nearest available slot suitable for the selected vehicle.";

        return new ParkingRecommendationDto
        {
            ParkingSlotId = best.ParkingSlotId,
            ParkingZoneId = best.ParkingZoneId,
            SlotCode = best.SlotCode,
            DistanceCost = best.DistanceCost,
            IsAccessible = best.IsAccessible,
            Reason = reason
        };
    }
}