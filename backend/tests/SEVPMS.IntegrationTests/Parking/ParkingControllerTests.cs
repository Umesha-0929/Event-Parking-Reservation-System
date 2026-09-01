using Microsoft.AspNetCore.Mvc;
using SEVPMS.Api.Controllers;
using SEVPMS.Application.Features.Parking.DTOs;
using SEVPMS.Application.Features.Parking.Interfaces;
using Xunit;

namespace SEVPMS.IntegrationTests.Parking;

public sealed class ParkingControllerTests
{
    [Fact]
    public async Task GetZonesByVenue_ReturnsOkWithZones()
    {
        var venueId = Guid.NewGuid();

        var service = new FakeParkingService
        {
            Zones =
            [
                new ParkingZoneDto
                {
                    Id = Guid.NewGuid(),
                    VenueId = venueId,
                    EventId = Guid.NewGuid(),
                    Name = "Zone A",
                    Level = "Basement 1",
                    EntranceName = "Gate 1"
                }
            ]
        };

        var controller = new ParkingController(service);

        var result = await controller.GetZonesByVenue(
            venueId,
            CancellationToken.None);

        var okResult =
            Assert.IsType<OkObjectResult>(result.Result);

        var zones =
            Assert.IsAssignableFrom<IReadOnlyList<ParkingZoneDto>>(
                okResult.Value);

        Assert.Single(zones);
    }

    [Fact]
    public async Task GetSlotsByZone_ReturnsOkWithSlots()
    {
        var zoneId = Guid.NewGuid();

        var service = new FakeParkingService
        {
            Slots =
            [
                new ParkingSlotDto
                {
                    Id = Guid.NewGuid(),
                    ParkingZoneId = zoneId,
                    EventId = Guid.NewGuid(),
                    SlotCode = "B2-128",
                    X = 120,
                    Y = 40,
                    IsAccessible = false,
                    Status = "Available"
                }
            ]
        };

        var controller = new ParkingController(service);

        var result = await controller.GetSlotsByZone(
            zoneId,
            CancellationToken.None);

        var okResult =
            Assert.IsType<OkObjectResult>(result.Result);

        var slots =
            Assert.IsAssignableFrom<IReadOnlyList<ParkingSlotDto>>(
                okResult.Value);

        Assert.Single(slots);
    }

    [Fact]
    public async Task GetSlotById_WhenSlotExists_ReturnsOk()
    {
        var slotId = Guid.NewGuid();

        var service = new FakeParkingService
        {
            Slot = new ParkingSlotDto
            {
                Id = slotId,
                ParkingZoneId = Guid.NewGuid(),
                SlotCode = "A-01",
                X = 10,
                Y = 20,
                IsAccessible = true,
                Status = "Available"
            }
        };

        var controller = new ParkingController(service);

        var result = await controller.GetSlotById(
            slotId,
            CancellationToken.None);

        var okResult =
            Assert.IsType<OkObjectResult>(result.Result);

        var slot =
            Assert.IsType<ParkingSlotDto>(okResult.Value);

        Assert.Equal(slotId, slot.Id);
    }

    [Fact]
    public async Task GetSlotById_WhenSlotDoesNotExist_ReturnsNotFound()
    {
        var service = new FakeParkingService();

        var controller = new ParkingController(service);

        var result = await controller.GetSlotById(
            Guid.NewGuid(),
            CancellationToken.None);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    private sealed class FakeParkingService
        : IParkingService
    {
        public IReadOnlyList<ParkingZoneDto> Zones { get; init; }
            = [];

        public IReadOnlyList<ParkingSlotDto> Slots { get; init; }
            = [];

        public ParkingSlotDto? Slot { get; init; }

        public Task<IReadOnlyList<ParkingZoneDto>> GetZonesByVenueAsync(
            Guid venueId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Zones);
        }

        public Task<IReadOnlyList<ParkingSlotDto>> GetSlotsByZoneAsync(
            Guid parkingZoneId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Slots);
        }

        public Task<ParkingSlotDto?> GetSlotByIdAsync(
            Guid parkingSlotId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Slot);
        }
    }
}