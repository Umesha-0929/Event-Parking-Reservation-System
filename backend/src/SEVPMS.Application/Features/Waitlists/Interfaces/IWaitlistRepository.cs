using SEVPMS.Domain.Entities.Waitlists;

namespace SEVPMS.Application.Features.Waitlists.Interfaces;

public interface IWaitlistRepository
{
    Task<WaitlistEntry?> GetByIdAsync(
        Guid waitlistEntryId,
        CancellationToken cancellationToken = default);

    Task<WaitlistEntry?> GetByEventAndCustomerAsync(
        Guid eventId,
        Guid customerUserId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<WaitlistEntry>> GetByEventAsync(
        Guid eventId,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        WaitlistEntry entry,
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(
        CancellationToken cancellationToken = default);
}