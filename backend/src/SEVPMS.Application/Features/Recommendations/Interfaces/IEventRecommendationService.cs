using SEVPMS.Application.Features.Recommendations.DTOs;

namespace SEVPMS.Application.Features.Recommendations.Interfaces;

public interface IEventRecommendationService
{
    Task<IReadOnlyList<EventRecommendationDto>>
        GetRecommendationsAsync(
            Guid customerUserId,
            EventRecommendationRequest request,
            CancellationToken cancellationToken = default);
}