using SEVPMS.Application.Features.Waitlists.DTOs;

namespace SEVPMS.Application.Features.Waitlists.Interfaces;

public interface IWaitlistService
{
    Task<WaitlistEntryDto?> GetMineAsync(
        Guid customerUserId,
        Guid eventId,
        CancellationToken cancellationToken = default);

    Task<WaitlistEntryDto> JoinAsync(
        Guid customerUserId,
        Guid eventId,
        CancellationToken cancellationToken = default);

    Task<bool> LeaveAsync(
        Guid customerUserId,
        Guid eventId,
        CancellationToken cancellationToken = default);

    Task<WaitlistEligibilityResultDto> NotifyNextEligibleAsync(
        Guid eventId,
        int availableCount,
        CancellationToken cancellationToken = default);
}