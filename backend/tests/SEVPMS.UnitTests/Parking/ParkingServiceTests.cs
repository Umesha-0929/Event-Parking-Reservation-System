using SEVPMS.Application.Features.Parking.DTOs;
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

    [Fact]
    public async Task CreateZoneAsync_WithValidRequest_CreatesZone()
    {
        var venueId = Guid.NewGuid();
        var eventId = Guid.NewGuid();

        var repository = new FakeParkingRepository();
        var service = new ParkingService(repository);

        var result = await service.CreateZoneAsync(
            new UpsertParkingZoneRequest
            {
                VenueId = venueId,
                EventId = eventId,
                Name = " Zone A ",
                Level = " Ground ",
                EntranceName = " Main Gate "
            });

        var created = Assert.Single(repository.Zones);

        Assert.Equal(venueId, created.VenueId);
        Assert.Equal(eventId, created.EventId);
        Assert.Equal("Zone A", created.Name);
        Assert.Equal("Ground", created.Level);
        Assert.Equal("Main Gate", created.EntranceName);

        Assert.Equal(created.Id, result.Id);
        Assert.True(repository.SaveChangesCalled);
    }

    [Fact]
    public async Task UpdateZoneAsync_WhenZoneExists_UpdatesZone()
    {
        var zoneId = Guid.NewGuid();
        var venueId = Guid.NewGuid();

        var repository = new FakeParkingRepository();

        repository.Zones.Add(new ParkingZone
        {
            Id = zoneId,
            VenueId = Guid.NewGuid(),
            Name = "Old Zone",
            Level = "Old Level",
            EntranceName = "Old Gate"
        });

        var service = new ParkingService(repository);

        var result = await service.UpdateZoneAsync(
            zoneId,
            new UpsertParkingZoneRequest
            {
                VenueId = venueId,
                Name = "New Zone",
                Level = "Level 2",
                EntranceName = "Gate 2"
            });

        Assert.Equal(zoneId, result.Id);
        Assert.Equal(venueId, result.VenueId);
        Assert.Equal("New Zone", result.Name);
        Assert.Equal("Level 2", result.Level);
        Assert.Equal("Gate 2", result.EntranceName);

        Assert.True(repository.UpdateZoneCalled);
        Assert.True(repository.SaveChangesCalled);
    }

    [Fact]
    public async Task DeleteZoneAsync_WhenZoneExists_RemovesZoneAndSlots()
    {
        var zoneId = Guid.NewGuid();

        var repository = new FakeParkingRepository();

        repository.Zones.Add(new ParkingZone
        {
            Id = zoneId,
            VenueId = Guid.NewGuid(),
            Name = "Zone A"
        });

        repository.Slots.Add(new ParkingSlot
        {
            Id = Guid.NewGuid(),
            ParkingZoneId = zoneId,
            SlotCode = "A-01",
            Status = "Available"
        });

        repository.Slots.Add(new ParkingSlot
        {
            Id = Guid.NewGuid(),
            ParkingZoneId = zoneId,
            SlotCode = "A-02",
            Status = "Available"
        });

        var service = new ParkingService(repository);

        var result = await service.DeleteZoneAsync(zoneId);

        Assert.True(result);
        Assert.Empty(repository.Zones);
        Assert.Empty(repository.Slots);
        Assert.True(repository.SaveChangesCalled);
    }

    [Fact]
    public async Task CreateSlotAsync_WithValidRequest_CreatesSlot()
    {
        var zoneId = Guid.NewGuid();
        var eventId = Guid.NewGuid();

        var repository = new FakeParkingRepository();

        repository.Zones.Add(new ParkingZone
        {
            Id = zoneId,
            VenueId = Guid.NewGuid(),
            EventId = eventId,
            Name = "Zone A"
        });

        var service = new ParkingService(repository);

        var result = await service.CreateSlotAsync(
            new UpsertParkingSlotRequest
            {
                ParkingZoneId = zoneId,
                EventId = eventId,
                SlotCode = " A-01 ",
                X = 15,
                Y = 25,
                IsAccessible = true,
                Status = "available"
            });

        var created = Assert.Single(repository.Slots);

        Assert.Equal(zoneId, created.ParkingZoneId);
        Assert.Equal(eventId, created.EventId);
        Assert.Equal("A-01", created.SlotCode);
        Assert.Equal(15, created.X);
        Assert.Equal(25, created.Y);
        Assert.True(created.IsAccessible);
        Assert.Equal("Available", created.Status);

        Assert.Equal(created.Id, result.Id);
        Assert.True(repository.SaveChangesCalled);
    }

    [Fact]
    public async Task UpdateSlotAsync_WhenSlotExists_UpdatesSlot()
    {
        var zoneId = Guid.NewGuid();
        var slotId = Guid.NewGuid();

        var repository = new FakeParkingRepository();

        repository.Zones.Add(new ParkingZone
        {
            Id = zoneId,
            VenueId = Guid.NewGuid(),
            Name = "Zone A"
        });

        repository.Slots.Add(new ParkingSlot
        {
            Id = slotId,
            ParkingZoneId = zoneId,
            SlotCode = "A-01",
            Status = "Available"
        });

        var service = new ParkingService(repository);

        var result = await service.UpdateSlotAsync(
            slotId,
            new UpsertParkingSlotRequest
            {
                ParkingZoneId = zoneId,
                SlotCode = "A-10",
                X = 100,
                Y = 200,
                IsAccessible = true,
                Status = "Blocked"
            });

        Assert.Equal(slotId, result.Id);
        Assert.Equal("A-10", result.SlotCode);
        Assert.Equal(100, result.X);
        Assert.Equal(200, result.Y);
        Assert.True(result.IsAccessible);
        Assert.Equal("Blocked", result.Status);

        Assert.True(repository.UpdateSlotCalled);
        Assert.True(repository.SaveChangesCalled);
    }

    [Fact]
    public async Task DeleteSlotAsync_WhenSlotExists_RemovesSlot()
    {
        var slotId = Guid.NewGuid();

        var repository = new FakeParkingRepository();

        repository.Slots.Add(new ParkingSlot
        {
            Id = slotId,
            ParkingZoneId = Guid.NewGuid(),
            SlotCode = "A-01",
            Status = "Available"
        });

        var service = new ParkingService(repository);

        var result = await service.DeleteSlotAsync(slotId);

        Assert.True(result);
        Assert.Empty(repository.Slots);
        Assert.True(repository.SaveChangesCalled);
    }

    private sealed class FakeParkingRepository
        : IParkingRepository
    {
        public List<ParkingZone> Zones { get; } = [];

        public List<ParkingSlot> Slots { get; } = [];

        public bool SaveChangesCalled { get; private set; }

        public bool UpdateZoneCalled { get; private set; }

        public bool UpdateSlotCalled { get; private set; }

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
            return Task.FromResult(
                Slots.SingleOrDefault(
                    slot => slot.Id == parkingSlotId));
        }

        public Task<ParkingZone?> GetZoneByIdAsync(
            Guid parkingZoneId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(
                Zones.SingleOrDefault(
                    zone => zone.Id == parkingZoneId));
        }

        public Task AddZoneAsync(
            ParkingZone zone,
            CancellationToken cancellationToken = default)
        {
            Zones.Add(zone);
            return Task.CompletedTask;
        }

        public Task AddSlotAsync(
            ParkingSlot slot,
            CancellationToken cancellationToken = default)
        {
            Slots.Add(slot);
            return Task.CompletedTask;
        }

        public void UpdateZone(
            ParkingZone zone)
        {
            UpdateZoneCalled = true;
        }

        public void UpdateSlot(
            ParkingSlot slot)
        {
            UpdateSlotCalled = true;
        }

        public void DeleteZone(
            ParkingZone zone)
        {
            Zones.Remove(zone);
        }

        public void DeleteSlot(
            ParkingSlot slot)
        {
            Slots.Remove(slot);
        }

        public Task SaveChangesAsync(
            CancellationToken cancellationToken = default)
        {
            SaveChangesCalled = true;
            return Task.CompletedTask;
        }
    }
}