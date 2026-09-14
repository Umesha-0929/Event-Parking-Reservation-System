using SEVPMS.Domain.Entities.Seats;
namespace SEVPMS.Application.Features.Seats.Interfaces;
public sealed record SeatInventorySnapshot(Seat Seat, DateTime? ActiveHoldExpiresAtUtc);
public sealed record SeatHoldAttempt(bool Succeeded, string HoldToken, Guid EventId, IReadOnlyCollection<Guid> SeatIds, DateTime ExpiresAtUtc, IReadOnlyCollection<Guid> ConflictingSeatIds);
public sealed record SeatHoldMutation(bool Changed, string HoldToken, Guid EventId, IReadOnlyCollection<Guid> SeatIds, DateTime? ExpiresAtUtc = null);
public interface ISeatInventoryRepository
{
    Task<IReadOnlyList<SeatInventorySnapshot>> GetAvailabilityAsync(Guid eventId, Guid? sectionId, DateTime nowUtc, CancellationToken cancellationToken = default);
    Task<SeatHoldAttempt> TryCreateOrRefreshHoldAsync(Guid eventId, Guid userId, IReadOnlyCollection<Guid> seatIds, string? existingHoldToken, DateTime nowUtc, DateTime expiresAtUtc, CancellationToken cancellationToken = default);
    Task<SeatHoldMutation> ReleaseHoldAsync(string holdToken, Guid userId, DateTime nowUtc, CancellationToken cancellationToken = default);
    Task<SeatHoldMutation> CommitHoldAsync(string holdToken, Guid userId, DateTime nowUtc, CancellationToken cancellationToken = default);
    Task<Seat> UpsertSeatAsync(Guid eventId, Seat seat, CancellationToken cancellationToken = default);
    Task<SeatViewAsset?> GetSeatViewAsync(Guid eventId, Guid seatId, CancellationToken cancellationToken = default);
    Task<SeatViewAsset> UpsertSeatViewAsync(Guid eventId, SeatViewAsset asset, CancellationToken cancellationToken = default);
}
