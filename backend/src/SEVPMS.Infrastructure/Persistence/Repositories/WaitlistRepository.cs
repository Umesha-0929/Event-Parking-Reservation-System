using Microsoft.EntityFrameworkCore;
using SEVPMS.Application.Features.Waitlists.Interfaces;
using SEVPMS.Domain.Entities.Waitlists;

namespace SEVPMS.Infrastructure.Persistence.Repositories;

public sealed class WaitlistRepository(
    SEVPMSDbContext dbContext)
    : IWaitlistRepository
{
    public Task<WaitlistEntry?> GetByIdAsync(
        Guid waitlistEntryId,
        CancellationToken cancellationToken = default)
        => dbContext.Set<WaitlistEntry>()
            .SingleOrDefaultAsync(
                x => x.Id == waitlistEntryId,
                cancellationToken);

    public Task<WaitlistEntry?> GetByEventAndCustomerAsync(
        Guid eventId,
        Guid customerUserId,
        CancellationToken cancellationToken = default)
        => dbContext.Set<WaitlistEntry>()
            .SingleOrDefaultAsync(
                x =>
                    x.EventId == eventId &&
                    x.CustomerUserId == customerUserId,
                cancellationToken);

    public async Task<IReadOnlyList<WaitlistEntry>>
        GetByEventAsync(
            Guid eventId,
            CancellationToken cancellationToken = default)
        => await dbContext.Set<WaitlistEntry>()
            .Where(x => x.EventId == eventId)
            .OrderBy(x => x.CreatedAtUtc)
            .ThenBy(x => x.Id)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(
        WaitlistEntry entry,
        CancellationToken cancellationToken = default)
        => await dbContext.Set<WaitlistEntry>()
            .AddAsync(entry, cancellationToken);

    public async Task SaveChangesAsync(
        CancellationToken cancellationToken = default)
        => await dbContext.SaveChangesAsync(
            cancellationToken);
}