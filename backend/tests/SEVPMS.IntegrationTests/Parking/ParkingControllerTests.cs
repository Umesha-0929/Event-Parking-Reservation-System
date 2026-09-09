using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SEVPMS.Api.Authorization;
using SEVPMS.Api.Controllers;
using SEVPMS.Application.Features.Parking.DTOs;
using SEVPMS.Application.Features.Parking.Interfaces;
using SEVPMS.Application.Interfaces.Repositories;
using SEVPMS.Domain.Entities.Parking;
using SEVPMS.Domain.Entities.Venues;
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

        var controller = CreateController(service);
        var result = await controller.GetZonesByVenue(venueId, 1, 50, CancellationToken.None);
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var zones = Assert.IsAssignableFrom<IReadOnlyList<ParkingZoneDto>>(okResult.Value);
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

        var controller = CreateController(service);
        var result = await controller.GetSlotsByZone(zoneId, 1, 100, CancellationToken.None);
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var slots = Assert.IsAssignableFrom<IReadOnlyList<ParkingSlotDto>>(okResult.Value);
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

        var controller = CreateController(service);
        var result = await controller.GetSlotById(slotId, CancellationToken.None);
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var slot = Assert.IsType<ParkingSlotDto>(okResult.Value);
        Assert.Equal(slotId, slot.Id);
    }

    [Fact]
    public async Task GetSlotById_WhenSlotDoesNotExist_ReturnsNotFound()
    {
        var controller = CreateController(new FakeParkingService());
        var result = await controller.GetSlotById(Guid.NewGuid(), CancellationToken.None);
        Assert.IsType<NotFoundResult>(result.Result);
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

        var controller = CreateController(service);
        var result = await controller.CreateZone(
            new UpsertParkingZoneRequest
            {
                VenueId = venueId,
                Name = "Zone A",
                Level = "Ground",
                EntranceName = "Main Gate"
            },
            CancellationToken.None);

        var created = Assert.IsType<CreatedAtActionResult>(result.Result);
        var zone = Assert.IsType<ParkingZoneDto>(created.Value);
        Assert.Equal("Zone A", zone.Name);
    }

    [Fact]
    public async Task UpdateZone_ReturnsOk()
    {
        var zoneId = Guid.NewGuid();
        var venueId = Guid.NewGuid();
        var service = new FakeParkingService
        {
            UpdatedZone = new ParkingZoneDto
            {
                Id = zoneId,
                VenueId = venueId,
                Name = "Updated Zone",
                Level = "Level 2",
                EntranceName = "Gate 2"
            }
        };
        var repository = new FakeParkingRepository();
        repository.Zones[zoneId] = new ParkingZone { Id = zoneId, VenueId = venueId };

        var controller = CreateController(service, repository);
        var result = await controller.UpdateZone(
            zoneId,
            new UpsertParkingZoneRequest
            {
                VenueId = venueId,
                Name = "Updated Zone",
                Level = "Level 2",
                EntranceName = "Gate 2"
            },
            CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var zone = Assert.IsType<ParkingZoneDto>(okResult.Value);
        Assert.Equal(zoneId, zone.Id);
        Assert.Equal("Updated Zone", zone.Name);
    }

    [Fact]
    public async Task DeleteZone_WhenZoneExists_ReturnsNoContent()
    {
        var zoneId = Guid.NewGuid();
        var venueId = Guid.NewGuid();
        var service = new FakeParkingService { DeleteZoneResult = true };
        var repository = new FakeParkingRepository();
        repository.Zones[zoneId] = new ParkingZone { Id = zoneId, VenueId = venueId };

        var controller = CreateController(service, repository);
        var result = await controller.DeleteZone(zoneId, CancellationToken.None);
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task CreateSlot_ReturnsCreated()
    {
        var zoneId = Guid.NewGuid();
        var venueId = Guid.NewGuid();
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
        var repository = new FakeParkingRepository();
        repository.Zones[zoneId] = new ParkingZone { Id = zoneId, VenueId = venueId };

        var controller = CreateController(service, repository);
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

        var created = Assert.IsType<CreatedAtActionResult>(result.Result);
        var slot = Assert.IsType<ParkingSlotDto>(created.Value);
        Assert.Equal("A-01", slot.SlotCode);
    }

    [Fact]
    public async Task UpdateSlot_ReturnsOk()
    {
        var slotId = Guid.NewGuid();
        var zoneId = Guid.NewGuid();
        var venueId = Guid.NewGuid();
        var service = new FakeParkingService
        {
            UpdatedSlot = new ParkingSlotDto
            {
                Id = slotId,
                ParkingZoneId = zoneId,
                SlotCode = "A-10",
                X = 100,
                Y = 200,
                IsAccessible = true,
                Status = "Blocked"
            }
        };
        var repository = new FakeParkingRepository();
        repository.Zones[zoneId] = new ParkingZone { Id = zoneId, VenueId = venueId };
        repository.Slots[slotId] = new ParkingSlot { Id = slotId, ParkingZoneId = zoneId };

        var controller = CreateController(service, repository);
        var result = await controller.UpdateSlot(
            slotId,
            new UpsertParkingSlotRequest
            {
                ParkingZoneId = zoneId,
                SlotCode = "A-10",
                X = 100,
                Y = 200,
                IsAccessible = true,
                Status = "Blocked"
            },
            CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var slot = Assert.IsType<ParkingSlotDto>(okResult.Value);
        Assert.Equal(slotId, slot.Id);
        Assert.Equal("A-10", slot.SlotCode);
    }

    [Fact]
    public async Task DeleteSlot_WhenSlotExists_ReturnsNoContent()
    {
        var slotId = Guid.NewGuid();
        var zoneId = Guid.NewGuid();
        var venueId = Guid.NewGuid();
        var service = new FakeParkingService { DeleteSlotResult = true };
        var repository = new FakeParkingRepository();
        repository.Zones[zoneId] = new ParkingZone { Id = zoneId, VenueId = venueId };
        repository.Slots[slotId] = new ParkingSlot { Id = slotId, ParkingZoneId = zoneId };

        var controller = CreateController(service, repository);
        var result = await controller.DeleteSlot(slotId, CancellationToken.None);
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task CreateZone_VenueOwnerOwnsVenue_ReturnsCreated()
    {
        var ownerId = Guid.NewGuid();
        var venueId = Guid.NewGuid();
        var service = new FakeParkingService
        {
            CreatedZone = new ParkingZoneDto
            {
                Id = Guid.NewGuid(),
                VenueId = venueId,
                Name = "Owner Zone",
                Level = "Ground",
                EntranceName = "Main"
            }
        };
        var venues = new FakeVenueRepository();
        venues.Venues[venueId] = new Venue { Id = venueId, OwnerUserId = ownerId, Name = "Owner Venue" };
        var controller = CreateController(service, venueRepository: venues, role: "VenueOwner", userId: ownerId);

        var result = await controller.CreateZone(
            new UpsertParkingZoneRequest
            {
                VenueId = venueId,
                Name = "Owner Zone",
                Level = "Ground",
                EntranceName = "Main"
            },
            CancellationToken.None);

        Assert.IsType<CreatedAtActionResult>(result.Result);
    }

    [Fact]
    public async Task CreateZone_VenueOwnerDoesNotOwnVenue_ReturnsForbid()
    {
        var venueId = Guid.NewGuid();
        var venues = new FakeVenueRepository();
        venues.Venues[venueId] = new Venue { Id = venueId, OwnerUserId = Guid.NewGuid(), Name = "Other Venue" };
        var controller = CreateController(
            new FakeParkingService(),
            venueRepository: venues,
            role: "VenueOwner",
            userId: Guid.NewGuid());

        var result = await controller.CreateZone(
            new UpsertParkingZoneRequest
            {
                VenueId = venueId,
                Name = "Forbidden Zone",
                Level = "Ground",
                EntranceName = "Main"
            },
            CancellationToken.None);

        Assert.IsType<ForbidResult>(result.Result);
    }

    [Fact]
    public void ParkingMutationEndpoints_RequireParkingManagerPolicy()
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
            var method = typeof(ParkingController).GetMethod(methodName);
            Assert.NotNull(method);
            var attribute = Assert.Single(
                method!
                    .GetCustomAttributes(typeof(AuthorizeAttribute), false)
                    .Cast<AuthorizeAttribute>());

            Assert.Equal(AuthorizationPolicies.ParkingManager, attribute.Policy);
        }
    }

    private static ParkingController CreateController(
        IParkingService service,
        FakeParkingRepository? parkingRepository = null,
        FakeVenueRepository? venueRepository = null,
        string role = "Admin",
        Guid? userId = null)
    {
        var controller = new ParkingController(
            service,
            parkingRepository ?? new FakeParkingRepository(),
            new FakeParkingRouteRepository(),
            venueRepository ?? new FakeVenueRepository());

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, (userId ?? Guid.NewGuid()).ToString()),
            new Claim(ClaimTypes.Role, role)
        };

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"))
            }
        };

        return controller;
    }

    private sealed class FakeParkingRouteRepository : IParkingRouteRepository
    {
        public Task<IReadOnlyList<ParkingNode>> GetNodesByVenueAsync(
            Guid venueId,
            CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<ParkingNode>>(Array.Empty<ParkingNode>());

        public Task<IReadOnlyList<ParkingEdge>> GetEdgesByVenueAsync(
            Guid venueId,
            CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<ParkingEdge>>(Array.Empty<ParkingEdge>());
    }

    private sealed class FakeParkingRepository : IParkingRepository
    {
        public Dictionary<Guid, ParkingZone> Zones { get; } = [];
        public Dictionary<Guid, ParkingSlot> Slots { get; } = [];

        public Task<IReadOnlyList<ParkingZone>> GetZonesByVenueAsync(Guid venueId, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<ParkingZone>>(Zones.Values.Where(zone => zone.VenueId == venueId).ToList());

        public Task<IReadOnlyList<ParkingSlot>> GetSlotsByZoneAsync(Guid parkingZoneId, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<ParkingSlot>>(Slots.Values.Where(slot => slot.ParkingZoneId == parkingZoneId).ToList());

        public Task<ParkingSlot?> GetSlotByIdAsync(Guid parkingSlotId, CancellationToken cancellationToken = default)
            => Task.FromResult(Slots.GetValueOrDefault(parkingSlotId));

        public Task<ParkingZone?> GetZoneByIdAsync(Guid parkingZoneId, CancellationToken cancellationToken = default)
            => Task.FromResult(Zones.GetValueOrDefault(parkingZoneId));

        public Task AddZoneAsync(ParkingZone zone, CancellationToken cancellationToken = default)
        {
            Zones[zone.Id] = zone;
            return Task.CompletedTask;
        }

        public Task AddSlotAsync(ParkingSlot slot, CancellationToken cancellationToken = default)
        {
            Slots[slot.Id] = slot;
            return Task.CompletedTask;
        }

        public void UpdateZone(ParkingZone zone) => Zones[zone.Id] = zone;
        public void UpdateSlot(ParkingSlot slot) => Slots[slot.Id] = slot;
        public void DeleteZone(ParkingZone zone) => Zones.Remove(zone.Id);
        public void DeleteSlot(ParkingSlot slot) => Slots.Remove(slot.Id);
        public Task SaveChangesAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private sealed class FakeVenueRepository : IVenueRepository
    {
        public Dictionary<Guid, Venue> Venues { get; } = [];

        public Task<IReadOnlyList<Venue>> GetAllAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<Venue>>(Venues.Values.ToList());

        public Task<IReadOnlyList<Venue>> GetByOwnerUserIdAsync(Guid ownerUserId, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<Venue>>(Venues.Values.Where(venue => venue.OwnerUserId == ownerUserId).ToList());

        public Task<Venue?> GetByIdAsync(Guid venueId, CancellationToken cancellationToken = default)
            => Task.FromResult(Venues.GetValueOrDefault(venueId));

        public Task AddAsync(Venue venue, CancellationToken cancellationToken = default)
        {
            Venues[venue.Id] = venue;
            return Task.CompletedTask;
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task<bool> IsReferencedAsync(
            Guid venueId,
            CancellationToken cancellationToken = default)
            => Task.FromResult(false);

        public Task DeleteAsync(
            Venue venueToDelete,
            CancellationToken cancellationToken = default)
            => Task.CompletedTask;
}

    private sealed class FakeParkingService : IParkingService
    {
        public IReadOnlyList<ParkingZoneDto> Zones { get; init; } = [];
        public IReadOnlyList<ParkingSlotDto> Slots { get; init; } = [];
        public ParkingSlotDto? Slot { get; init; }
        public ParkingZoneDto? CreatedZone { get; init; }
        public ParkingZoneDto? UpdatedZone { get; init; }
        public ParkingSlotDto? CreatedSlot { get; init; }
        public ParkingSlotDto? UpdatedSlot { get; init; }
        public bool DeleteZoneResult { get; init; }
        public bool DeleteSlotResult { get; init; }

        public Task<IReadOnlyList<ParkingZoneDto>> GetZonesByVenueAsync(Guid venueId, CancellationToken cancellationToken = default)
            => Task.FromResult(Zones);

        public Task<IReadOnlyList<ParkingSlotDto>> GetSlotsByZoneAsync(Guid parkingZoneId, CancellationToken cancellationToken = default)
            => Task.FromResult(Slots);

        public Task<ParkingSlotDto?> GetSlotByIdAsync(Guid parkingSlotId, CancellationToken cancellationToken = default)
            => Task.FromResult(Slot);

        public Task<ParkingZoneDto> CreateZoneAsync(UpsertParkingZoneRequest request, CancellationToken cancellationToken = default)
            => Task.FromResult(CreatedZone ?? throw new InvalidOperationException());

        public Task<ParkingZoneDto> UpdateZoneAsync(Guid parkingZoneId, UpsertParkingZoneRequest request, CancellationToken cancellationToken = default)
            => Task.FromResult(UpdatedZone ?? throw new InvalidOperationException());

        public Task<bool> DeleteZoneAsync(Guid parkingZoneId, CancellationToken cancellationToken = default)
            => Task.FromResult(DeleteZoneResult);

        public Task<ParkingSlotDto> CreateSlotAsync(UpsertParkingSlotRequest request, CancellationToken cancellationToken = default)
            => Task.FromResult(CreatedSlot ?? throw new InvalidOperationException());

        public Task<IReadOnlyList<ParkingSlotDto>> CreateSlotsBulkAsync(
            IReadOnlyCollection<UpsertParkingSlotRequest> requests,
            CancellationToken cancellationToken = default)
        {
            if (requests.Count == 0)
            {
                return Task.FromResult<IReadOnlyList<ParkingSlotDto>>(Array.Empty<ParkingSlotDto>());
            }

            var slot = CreatedSlot ?? throw new InvalidOperationException();
            return Task.FromResult<IReadOnlyList<ParkingSlotDto>>(
                Enumerable.Repeat(slot, requests.Count).ToArray());
        }

        public Task<ParkingSlotDto> UpdateSlotAsync(Guid parkingSlotId, UpsertParkingSlotRequest request, CancellationToken cancellationToken = default)
            => Task.FromResult(UpdatedSlot ?? throw new InvalidOperationException());

        public Task<bool> DeleteSlotAsync(Guid parkingSlotId, CancellationToken cancellationToken = default)
            => Task.FromResult(DeleteSlotResult);
    }
}
