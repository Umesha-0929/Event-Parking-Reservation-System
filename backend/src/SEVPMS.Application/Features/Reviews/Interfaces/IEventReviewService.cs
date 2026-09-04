using SEVPMS.Application.Features.Reviews.DTOs;

namespace SEVPMS.Application.Features.Reviews.Interfaces;

public interface IEventReviewService
{
    Task<EventReviewDto> CreateAsync(
        Guid customerUserId,
        Guid eventId,
        CreateEventReviewRequest request,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<EventReviewDto>> GetByEventAsync(
        Guid eventId,
        CancellationToken cancellationToken = default);

    Task<EventRatingSummaryDto> GetSummaryAsync(
        Guid eventId,
        CancellationToken cancellationToken = default);
}