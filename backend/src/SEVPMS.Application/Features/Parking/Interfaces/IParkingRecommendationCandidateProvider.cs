using SEVPMS.Application.Features.Parking.DTOs;

namespace SEVPMS.Application.Features.Parking.Interfaces;

public interface IParkingRecommendationCandidateProvider
{
    Task<IReadOnlyList<ParkingRecommendationCandidateDto>> GetCandidatesAsync(
        ParkingRecommendationRequest request,
        Guid userId,
        CancellationToken cancellationToken = default);
}