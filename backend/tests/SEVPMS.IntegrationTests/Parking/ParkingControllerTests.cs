using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SEVPMS.Api.Authorization;
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
            Assert.IsType<ParkingSlotDto>(
                okResult.Value);

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

        Assert.IsType<NotFoundResult>(
            result.Result);
    }

    [Fact]
    public async Task CreateZone_ReturnsCreated()
    {
        var venueId = Guid.NewGuid();

        var service = new FakeParkingService
        {
            CreatedZone = new ParkingZoneDto
            {
                Id = Guid.NewGuid(),
                VenueId = venueId,
                Name = "Zone A",
                Level = "Ground",
                EntranceName = "Main Gate"
            }
        };

        var controller = new ParkingController(service);

        var result = await controller.CreateZone(
            new UpsertParkingZoneRequest
            {
                VenueId = venueId,
                Name = "Zone A",
                Level = "Ground",
                EntranceName = "Main Gate"
            },
            CancellationToken.None);

        var created =
            Assert.IsType<CreatedAtActionResult>(
                result.Result);

        var zone =
            Assert.IsType<ParkingZoneDto>(
                created.Value);

        Assert.Equal("Zone A", zone.Name);
    }

    [Fact]
    public async Task UpdateZone_ReturnsOk()
    {
        var zoneId = Guid.NewGuid();

        var service = new FakeParkingService
        {
            UpdatedZone = new ParkingZoneDto
            {
                Id = zoneId,
                VenueId = Guid.NewGuid(),
                Name = "Updated Zone",
                Level = "Level 2",
                EntranceName = "Gate 2"
            }
        };

        var controller = new ParkingController(service);

        var result = await controller.UpdateZone(
            zoneId,
            new UpsertParkingZoneRequest
            {
                VenueId = service.UpdatedZone.VenueId,
                Name = "Updated Zone",
                Level = "Level 2",
                EntranceName = "Gate 2"
            },
            CancellationToken.None);

        var okResult =
            Assert.IsType<OkObjectResult>(
                result.Result);

        var zone =
            Assert.IsType<ParkingZoneDto>(
                okResult.Value);

        Assert.Equal(zoneId, zone.Id);
        Assert.Equal("Updated Zone", zone.Name);
    }

    [Fact]
    public async Task DeleteZone_WhenZoneExists_ReturnsNoContent()
    {
        var service = new FakeParkingService
        {
            DeleteZoneResult = true
        };

        var controller = new ParkingController(service);

        var result = await controller.DeleteZone(
            Guid.NewGuid(),
            CancellationToken.None);

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task CreateSlot_ReturnsCreated()
    {
        var zoneId = Guid.NewGuid();

        var service = new FakeParkingService
        {
            CreatedSlot = new ParkingSlotDto
            {
                Id = Guid.NewGuid(),
                ParkingZoneId = zoneId,
                SlotCode = "A-01",
                X = 10,
                Y = 20,
                IsAccessible = false,
                Status = "Available"
            }
        };

        var controller = new ParkingController(service);

        var result = await controller.CreateSlot(
            new UpsertParkingSlotRequest
            {
                ParkingZoneId = zoneId,
                SlotCode = "A-01",
                X = 10,
                Y = 20,
                IsAccessible = false,
                Status = "Available"
            },
            CancellationToken.None);

        var created =
            Assert.IsType<CreatedAtActionResult>(
                result.Result);

        var slot =
            Assert.IsType<ParkingSlotDto>(
                created.Value);

        Assert.Equal("A-01", slot.SlotCode);
    }

    [Fact]
    public async Task UpdateSlot_ReturnsOk()
    {
        var slotId = Guid.NewGuid();

        var service = new FakeParkingService
        {
            UpdatedSlot = new ParkingSlotDto
            {
                Id = slotId,
                ParkingZoneId = Guid.NewGuid(),
                SlotCode = "A-10",
                X = 100,
                Y = 200,
                IsAccessible = true,
                Status = "Blocked"
            }
        };

        var controller = new ParkingController(service);

        var result = await controller.UpdateSlot(
            slotId,
            new UpsertParkingSlotRequest
            {
                ParkingZoneId =
                    service.UpdatedSlot.ParkingZoneId,
                SlotCode = "A-10",
                X = 100,
                Y = 200,
                IsAccessible = true,
                Status = "Blocked"
            },
            CancellationToken.None);

        var okResult =
            Assert.IsType<OkObjectResult>(
                result.Result);

        var slot =
            Assert.IsType<ParkingSlotDto>(
                okResult.Value);

        Assert.Equal(slotId, slot.Id);
        Assert.Equal("A-10", slot.SlotCode);
    }

    [Fact]
    public async Task DeleteSlot_WhenSlotExists_ReturnsNoContent()
    {
        var service = new FakeParkingService
        {
            DeleteSlotResult = true
        };

        var controller = new ParkingController(service);

        var result = await controller.DeleteSlot(
            Guid.NewGuid(),
            CancellationToken.None);

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public void ParkingMutationEndpoints_RequireAdminPolicy()
    {
        var methodNames = new[]
        {
            nameof(ParkingController.CreateZone),
            nameof(ParkingController.UpdateZone),
            nameof(ParkingController.DeleteZone),
            nameof(ParkingController.CreateSlot),
            nameof(ParkingController.UpdateSlot),
            nameof(ParkingController.DeleteSlot)
        };

        foreach (var methodName in methodNames)
        {
            var method =
                typeof(ParkingController)
                    .GetMethod(methodName);

            Assert.NotNull(method);

            var attribute =
                Assert.Single(
                    method!
                        .GetCustomAttributes(
                            typeof(AuthorizeAttribute),
                            false)
                        .Cast<AuthorizeAttribute>());

            Assert.Equal(
                AuthorizationPolicies.AdminOnly,
                attribute.Policy);
        }
    }

    private sealed class FakeParkingService
        : IParkingService
    {
        public IReadOnlyList<ParkingZoneDto> Zones { get; init; }
            = [];

        public IReadOnlyList<ParkingSlotDto> Slots { get; init; }
            = [];

        public ParkingSlotDto? Slot { get; init; }

        public ParkingZoneDto? CreatedZone { get; init; }

        public ParkingZoneDto? UpdatedZone { get; init; }

        public ParkingSlotDto? CreatedSlot { get; init; }

        public ParkingSlotDto? UpdatedSlot { get; init; }

        public bool DeleteZoneResult { get; init; }

        public bool DeleteSlotResult { get; init; }

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

        public Task<ParkingZoneDto> CreateZoneAsync(
            UpsertParkingZoneRequest request,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(
                CreatedZone
                ?? throw new InvalidOperationException());
        }

        public Task<ParkingZoneDto> UpdateZoneAsync(
            Guid parkingZoneId,
            UpsertParkingZoneRequest request,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(
                UpdatedZone
                ?? throw new InvalidOperationException());
        }

        public Task<bool> DeleteZoneAsync(
            Guid parkingZoneId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(
                DeleteZoneResult);
        }

        public Task<ParkingSlotDto> CreateSlotAsync(
            UpsertParkingSlotRequest request,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(
                CreatedSlot
                ?? throw new InvalidOperationException());
        }

        public Task<ParkingSlotDto> UpdateSlotAsync(
            Guid parkingSlotId,
            UpsertParkingSlotRequest request,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(
                UpdatedSlot
                ?? throw new InvalidOperationException());
        }

        public Task<bool> DeleteSlotAsync(
            Guid parkingSlotId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(
                DeleteSlotResult);
        }
    }
}