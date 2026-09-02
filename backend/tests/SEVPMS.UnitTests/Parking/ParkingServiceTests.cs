using SEVPMS.Application.Features.Parking.Interfaces;
using SEVPMS.Application.Features.Parking.Services;
using SEVPMS.Domain.Entities.Parking;
using Xunit;

namespace SEVPMS.UnitTests.Parking;

public sealed class ParkingServiceTests
{
    [Fact]
    public async Task GetZonesByVenueAsync_ReturnsMappedZones()
    {
        var venueId = Guid.NewGuid();

        var repository = new FakeParkingRepository();

        repository.Zones.Add(new ParkingZone
        {
            VenueId = venueId,
            EventId = Guid.NewGuid(),
            Name = "Zone A",
            Level = "Basement 1",
            EntranceName = "Gate 1"
        });

        var service = new ParkingService(repository);

        var result = await service.GetZonesByVenueAsync(venueId);

        var zone = Assert.Single(result);

        Assert.Equal(venueId, zone.VenueId);
        Assert.Equal("Zone A", zone.Name);
        Assert.Equal("Basement 1", zone.Level);
        Assert.Equal("Gate 1", zone.EntranceName);
    }

    [Fact]
    public async Task GetSlotsByZoneAsync_ReturnsMappedSlots()
    {
        var zoneId = Guid.NewGuid();

        var repository = new FakeParkingRepository();

        repository.Slots.Add(new ParkingSlot
        {
            ParkingZoneId = zoneId,
            EventId = Guid.NewGuid(),
            SlotCode = "B2-128",
            X = 120,
            Y = 40,
            IsAccessible = true,
            Status = "Available"
        });

        var service = new ParkingService(repository);

        var result = await service.GetSlotsByZoneAsync(zoneId);

        var slot = Assert.Single(result);

        Assert.Equal(zoneId, slot.ParkingZoneId);
        Assert.Equal("B2-128", slot.SlotCode);
        Assert.Equal(120, slot.X);
        Assert.Equal(40, slot.Y);
        Assert.True(slot.IsAccessible);
        Assert.Equal("Available", slot.Status);
    }

    [Fact]
    public async Task GetSlotByIdAsync_WhenSlotNotFound_ReturnsNull()
    {
        var repository = new FakeParkingRepository();
        var service = new ParkingService(repository);

        var result = await service.GetSlotByIdAsync(
            Guid.NewGuid());

        Assert.Null(result);
    }

    private sealed class FakeParkingRepository
        : IParkingRepository
    {
        public List<ParkingZone> Zones { get; } = [];

        public List<ParkingSlot> Slots { get; } = [];

        public Task<IReadOnlyList<ParkingZone>> GetZonesByVenueAsync(
            Guid venueId,
            CancellationToken cancellationToken = default)
        {
            IReadOnlyList<ParkingZone> result = Zones
                .Where(zone => zone.VenueId == venueId)
                .ToList();

            return Task.FromResult(result);
        }

        public Task<IReadOnlyList<ParkingSlot>> GetSlotsByZoneAsync(
            Guid parkingZoneId,
            CancellationToken cancellationToken = default)
        {
            IReadOnlyList<ParkingSlot> result = Slots
                .Where(slot => slot.ParkingZoneId == parkingZoneId)
                .ToList();

            return Task.FromResult(result);
        }

        public Task<ParkingSlot?> GetSlotByIdAsync(
            Guid parkingSlotId,
            CancellationToken cancellationToken = default)
        {
            var result = Slots
                .SingleOrDefault(slot => slot.Id == parkingSlotId);

            return Task.FromResult(result);
        }

        public Task SaveChangesAsync(
            CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }
}