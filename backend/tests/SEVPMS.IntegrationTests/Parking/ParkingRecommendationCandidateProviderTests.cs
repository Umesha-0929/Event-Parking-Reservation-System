using SEVPMS.Application.Features.Parking.DTOs;
using SEVPMS.Application.Features.Parking.Interfaces;
using SEVPMS.Application.Features.Vehicles.Interfaces;
using SEVPMS.Domain.Entities.Parking;
using SEVPMS.Domain.Entities.Vehicles;
using SEVPMS.Infrastructure.Persistence.Providers;
using Xunit;

namespace SEVPMS.IntegrationTests.Parking;

public sealed class ParkingRecommendationCandidateProviderTests
{
    [Fact]
    public async Task GetCandidatesAsync_BuildsCandidateWithDistanceAndAvailability()
    {
        var venueId = Guid.NewGuid();
        var entranceId = Guid.NewGuid();

        var entrance = new ParkingNode
        {
            Id = entranceId,
            VenueId = venueId,
            NodeCode = "ENT-1",
            X = 0,
            Y = 0,
            NodeType = "Entrance"
        };

        var zone = new ParkingZone
        {
            Id = Guid.NewGuid(),
            VenueId = venueId,
            Name = "Zone A",
            Level = "Ground",
            EntranceName = "Main"
        };

        var slot = new ParkingSlot
        {
            Id = Guid.NewGuid(),
            ParkingZoneId = zone.Id,
            SlotCode = "A-01",
            X = 3,
            Y = 4,
            Status = "Available",
            IsAccessible = false
        };

        var provider = new ParkingRecommendationCandidateProvider(
            new FakeParkingRepository([zone], [slot]),
            new FakeRouteRepository([entrance]),
            new FakeVehicleRepository());

        var result = await provider.GetCandidatesAsync(
            new ParkingRecommendationRequest
            {
                VenueId = venueId,
                EntranceNodeId = entranceId
            },
            Guid.NewGuid());

        var candidate = Assert.Single(result);

        Assert.Equal(slot.Id, candidate.ParkingSlotId);
        Assert.Equal("A-01", candidate.SlotCode);
        Assert.Equal(5m, candidate.DistanceCost);
        Assert.True(candidate.IsAvailable);
        Assert.True(candidate.IsVehicleSuitable);
    }

    [Fact]
    public async Task GetCandidatesAsync_WhenEntranceDoesNotExist_ReturnsEmpty()
    {
        var provider = new ParkingRecommendationCandidateProvider(
            new FakeParkingRepository([], []),
            new FakeRouteRepository([]),
            new FakeVehicleRepository());

        var result = await provider.GetCandidatesAsync(
            new ParkingRecommendationRequest
            {
                VenueId = Guid.NewGuid(),
                EntranceNodeId = Guid.NewGuid()
            },
            Guid.NewGuid());

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetCandidatesAsync_WhenVehicleBelongsToAnotherUser_ReturnsEmpty()
    {
        var venueId = Guid.NewGuid();
        var vehicleId = Guid.NewGuid();

        var entrance = new ParkingNode
        {
            VenueId = venueId,
            NodeCode = "ENT-1",
            NodeType = "Entrance"
        };

        var vehicle = new SavedVehicle
        {
            Id = vehicleId,
            UserId = Guid.NewGuid(),
            Nickname = "Car",
            RegistrationNo = "ABC-1234",
            VehicleType = "Car"
        };

        var provider = new ParkingRecommendationCandidateProvider(
            new FakeParkingRepository([], []),
            new FakeRouteRepository([entrance]),
            new FakeVehicleRepository(vehicle));

        var result = await provider.GetCandidatesAsync(
            new ParkingRecommendationRequest
            {
                VenueId = venueId,
                EntranceNodeId = entrance.Id,
                SavedVehicleId = vehicleId
            },
            Guid.NewGuid());

        Assert.Empty(result);
    }

    private sealed class FakeParkingRepository(
        IReadOnlyList<ParkingZone> zones,
        IReadOnlyList<ParkingSlot> slots)
        : IParkingRepository
    {
        public Task<IReadOnlyList<ParkingZone>> GetZonesByVenueAsync(
            Guid venueId,
            CancellationToken cancellationToken = default)
            => Task.FromResult(zones);

        public Task<IReadOnlyList<ParkingSlot>> GetSlotsByZoneAsync(
            Guid parkingZoneId,
            CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<ParkingSlot>>(
                slots
                    .Where(x => x.ParkingZoneId == parkingZoneId)
                    .ToList());

        public Task<ParkingSlot?> GetSlotByIdAsync(
            Guid parkingSlotId,
            CancellationToken cancellationToken = default)
            => Task.FromResult(
                slots.SingleOrDefault(x => x.Id == parkingSlotId));

        public Task SaveChangesAsync(
            CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }

    private sealed class FakeRouteRepository(
        IReadOnlyList<ParkingNode> nodes)
        : IParkingRouteRepository
    {
        public Task<IReadOnlyList<ParkingNode>> GetNodesByVenueAsync(
            Guid venueId,
            CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<ParkingNode>>(
                nodes
                    .Where(x => x.VenueId == venueId)
                    .ToList());

        public Task<IReadOnlyList<ParkingEdge>> GetEdgesByVenueAsync(
            Guid venueId,
            CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<ParkingEdge>>([]);
    }

    private sealed class FakeVehicleRepository(
        SavedVehicle? vehicle = null)
        : ISavedVehicleRepository
    {
        public Task<IReadOnlyList<SavedVehicle>> GetByUserIdAsync(
            Guid userId,
            CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<SavedVehicle>>([]);

        public Task<SavedVehicle?> GetByIdAsync(
            Guid vehicleId,
            CancellationToken cancellationToken = default)
            => Task.FromResult(
                vehicle?.Id == vehicleId ? vehicle : null);

        public Task AddAsync(
            SavedVehicle vehicle,
            CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public void Update(SavedVehicle vehicle)
        {
        }

        public void Remove(SavedVehicle vehicle)
        {
        }

        public Task SaveChangesAsync(
            CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }
}