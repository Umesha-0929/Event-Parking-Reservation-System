using SEVPMS.Domain.Entities.Reviews;

namespace SEVPMS.Application.Features.Reviews.Interfaces;

public interface IEventReviewRepository
{
    Task<EventReview?> GetByEventAndCustomerAsync(
        Guid eventId,
        Guid customerUserId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<EventReview>> GetByEventAsync(
        Guid eventId,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        EventReview review,
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(
        CancellationToken cancellationToken = default);
}