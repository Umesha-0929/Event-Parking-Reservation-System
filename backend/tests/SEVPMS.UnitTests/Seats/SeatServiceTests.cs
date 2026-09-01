using SEVPMS.Application.Features.Seats.DTOs;
using SEVPMS.Application.Features.Seats.Interfaces;
using SEVPMS.Application.Features.Seats.Services;
using SEVPMS.Domain.Entities.Seats;
using SEVPMS.Domain.Enums;
namespace SEVPMS.UnitTests.Seats;
public sealed class SeatServiceTests
{
    [Fact]
    public async Task HoldAsync_ReturnsConflict_WhenRepositoryReportsConflict()
    {
        var eventId = Guid.NewGuid(); var userId = Guid.NewGuid(); var seatId = Guid.NewGuid();
        var repo = new FakeRepo { HoldAttempt = new(false, "HLD-X", eventId, new[] { seatId }, DateTime.UtcNow.AddMinutes(5), new[] { seatId }) };
        var service = new SeatService(repo, new FakeRealtime(), TimeProvider.System);
        var result = await service.HoldAsync(eventId, userId, new CreateSeatHoldRequest(new[] { seatId }));
        Assert.False(result.Succeeded); Assert.Contains(seatId, result.ConflictingSeatIds); Assert.Equal("seat_conflict", result.ErrorCode);
    }

    [Fact]
    public async Task Availability_ShowsHeld_WhenActiveHoldExists()
    {
        var eventId = Guid.NewGuid(); var seat = new Seat { EventId = eventId, SectionId = Guid.NewGuid(), RowLabel = "A", SeatNumber = "1", Status = SeatStatus.Available };
        var repo = new FakeRepo { Snapshots = new[] { new SeatInventorySnapshot(seat, DateTime.UtcNow.AddMinutes(3)) } };
        var service = new SeatService(repo, new FakeRealtime(), TimeProvider.System);
        var result = await service.GetAvailabilityAsync(eventId, null);
        Assert.Equal("Held", result.Single().State);
    }

    private sealed class FakeRealtime : ISeatRealtimeNotifier { public Task PublishSeatStateChangedAsync(Guid eventId, IReadOnlyCollection<Guid> seatIds, string state, DateTime? expiresAtUtc, CancellationToken cancellationToken = default) => Task.CompletedTask; }
    private sealed class FakeRepo : ISeatInventoryRepository
    {
        public IReadOnlyList<SeatInventorySnapshot> Snapshots { get; set; } = Array.Empty<SeatInventorySnapshot>();
        public SeatHoldAttempt HoldAttempt { get; set; } = new(true, "HLD", Guid.NewGuid(), Array.Empty<Guid>(), DateTime.UtcNow.AddMinutes(5), Array.Empty<Guid>());
        public Task<IReadOnlyList<SeatInventorySnapshot>> GetAvailabilityAsync(Guid eventId, Guid? sectionId, DateTime nowUtc, CancellationToken cancellationToken = default) => Task.FromResult(Snapshots);
        public Task<SeatHoldAttempt> TryCreateOrRefreshHoldAsync(Guid eventId, Guid userId, IReadOnlyCollection<Guid> seatIds, string? existingHoldToken, DateTime nowUtc, DateTime expiresAtUtc, CancellationToken cancellationToken = default) => Task.FromResult(HoldAttempt);
        public Task<SeatHoldMutation> ReleaseHoldAsync(string holdToken, Guid userId, DateTime nowUtc, CancellationToken cancellationToken = default) => Task.FromResult(new SeatHoldMutation(false, holdToken, Guid.Empty, Array.Empty<Guid>()));
        public Task<SeatHoldMutation> CommitHoldAsync(string holdToken, Guid userId, DateTime nowUtc, CancellationToken cancellationToken = default) => Task.FromResult(new SeatHoldMutation(false, holdToken, Guid.Empty, Array.Empty<Guid>()));
        public Task<Seat> UpsertSeatAsync(Guid eventId, Seat seat, CancellationToken cancellationToken = default) => Task.FromResult(seat);
        public Task<SeatViewAsset?> GetSeatViewAsync(Guid eventId, Guid seatId, CancellationToken cancellationToken = default) => Task.FromResult<SeatViewAsset?>(null);
        public Task<SeatViewAsset> UpsertSeatViewAsync(Guid eventId, SeatViewAsset asset, CancellationToken cancellationToken = default) => Task.FromResult(asset);
    }
}
