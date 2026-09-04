using Microsoft.EntityFrameworkCore;
using SEVPMS.Application.Features.Reviews.Interfaces;
using SEVPMS.Domain.Entities.Reviews;

namespace SEVPMS.Infrastructure.Persistence.Repositories;

public sealed class EventReviewRepository(
    SEVPMSDbContext dbContext)
    : IEventReviewRepository
{
    public Task<EventReview?>
        GetByEventAndCustomerAsync(
            Guid eventId,
            Guid customerUserId,
            CancellationToken cancellationToken = default)
        => dbContext.Set<EventReview>()
            .SingleOrDefaultAsync(
                x =>
                    x.EventId == eventId &&
                    x.CustomerUserId == customerUserId,
                cancellationToken);

    public async Task<IReadOnlyList<EventReview>>
        GetByEventAsync(
            Guid eventId,
            CancellationToken cancellationToken = default)
        => await dbContext.Set<EventReview>()
            .Where(x => x.EventId == eventId)
            .OrderByDescending(x => x.CreatedAtUtc)
            .ThenByDescending(x => x.Id)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(
        EventReview review,
        CancellationToken cancellationToken = default)
        => await dbContext.Set<EventReview>()
            .AddAsync(
                review,
                cancellationToken);

    public async Task SaveChangesAsync(
        CancellationToken cancellationToken = default)
        => await dbContext.SaveChangesAsync(
            cancellationToken);
}
