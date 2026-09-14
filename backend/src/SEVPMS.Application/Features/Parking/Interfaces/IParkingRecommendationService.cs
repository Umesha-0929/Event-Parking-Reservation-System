using SEVPMS.Application.Features.Parking.DTOs;

namespace SEVPMS.Application.Features.Parking.Interfaces;

public interface IParkingRecommendationService
{
    ParkingRecommendationDto? RecommendBestSlot(
        IReadOnlyList<ParkingRecommendationCandidateDto> candidates,
        bool requiresAccessibleParking);
}