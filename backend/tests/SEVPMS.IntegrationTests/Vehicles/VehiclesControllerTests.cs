using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SEVPMS.Api.Controllers;
using SEVPMS.Application.Features.Vehicles.DTOs;
using SEVPMS.Application.Features.Vehicles.Interfaces;
using SEVPMS.Application.Features.Vehicles.Validators;
using Xunit;

namespace SEVPMS.IntegrationTests.Vehicles;

public sealed class VehiclesControllerTests
{
    [Fact]
    public async Task GetAll_WhenUserClaimMissing_ReturnsUnauthorized()
    {
        var service = new FakeSavedVehicleService();
        var controller = CreateController(service);

        var result = await controller.GetAll(
            CancellationToken.None);

        Assert.IsType<UnauthorizedResult>(result.Result);
    }

    [Fact]
    public async Task GetAll_WhenUserAuthenticated_ReturnsVehicles()
    {
        var userId = Guid.NewGuid();

        var service = new FakeSavedVehicleService
        {
            Vehicles =
            [
                new SavedVehicleDto
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    Nickname = "My Car",
                    RegistrationNo = "WP CAB 1234",
                    VehicleType = "Car",
                    IsDefault = true
                }
            ]
        };

        var controller = CreateController(service, userId);

        var result = await controller.GetAll(
            CancellationToken.None);

        var okResult =
            Assert.IsType<OkObjectResult>(result.Result);

        var vehicles =
            Assert.IsAssignableFrom<IReadOnlyList<SavedVehicleDto>>(
                okResult.Value);

        Assert.Single(vehicles);
    }

    [Fact]
    public async Task GetById_WhenVehicleNotFound_ReturnsNotFound()
    {
        var userId = Guid.NewGuid();

        var service = new FakeSavedVehicleService
        {
            GetByIdResult = null
        };

        var controller = CreateController(service, userId);

        var result = await controller.GetById(
            Guid.NewGuid(),
            CancellationToken.None);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task Create_WithValidRequest_ReturnsCreatedAtAction()
    {
        var userId = Guid.NewGuid();

        var createdVehicle = new SavedVehicleDto
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Nickname = "My Car",
            RegistrationNo = "WP CAB 1234",
            VehicleType = "Car",
            IsDefault = true
        };

        var service = new FakeSavedVehicleService
        {
            CreateResult = createdVehicle
        };

        var controller = CreateController(service, userId);

        var request = new CreateSavedVehicleRequest
        {
            Nickname = "My Car",
            RegistrationNo = "WP CAB 1234",
            VehicleType = "Car",
            IsDefault = true
        };

        var result = await controller.Create(
            request,
            CancellationToken.None);

        var createdResult =
            Assert.IsType<CreatedAtActionResult>(result.Result);

        Assert.Equal(
            nameof(VehiclesController.GetById),
            createdResult.ActionName);

        Assert.Equal(createdVehicle, createdResult.Value);
    }

    [Fact]
    public async Task Create_WhenValidationFails_ReturnsBadRequest()
    {
        var userId = Guid.NewGuid();

        var service = new FakeSavedVehicleService
        {
            ThrowValidationException = true
        };

        var controller = CreateController(service, userId);

        var request = new CreateSavedVehicleRequest();

        var result = await controller.Create(
            request,
            CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task Delete_WhenVehicleDeleted_ReturnsNoContent()
    {
        var userId = Guid.NewGuid();

        var service = new FakeSavedVehicleService
        {
            DeleteResult = true
        };

        var controller = CreateController(service, userId);

        var result = await controller.Delete(
            Guid.NewGuid(),
            CancellationToken.None);

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task Delete_WhenVehicleNotFound_ReturnsNotFound()
    {
        var userId = Guid.NewGuid();

        var service = new FakeSavedVehicleService
        {
            DeleteResult = false
        };

        var controller = CreateController(service, userId);

        var result = await controller.Delete(
            Guid.NewGuid(),
            CancellationToken.None);

        Assert.IsType<NotFoundResult>(result);
    }

    private static VehiclesController CreateController(
        ISavedVehicleService service,
        Guid? userId = null)
    {
        var controller = new VehiclesController(service);

        var identity = new ClaimsIdentity();

        if (userId.HasValue)
        {
            identity.AddClaim(
                new Claim(
                    ClaimTypes.NameIdentifier,
                    userId.Value.ToString()));
        }

        controller.ControllerContext =
            new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(identity)
                }
            };

        return controller;
    }

    private sealed class FakeSavedVehicleService
        : ISavedVehicleService
    {
        public IReadOnlyList<SavedVehicleDto> Vehicles { get; init; }
            = [];

        public SavedVehicleDto? GetByIdResult { get; init; }

        public SavedVehicleDto? CreateResult { get; init; }

        public bool DeleteResult { get; init; }

        public bool ThrowValidationException { get; init; }

        public Task<IReadOnlyList<SavedVehicleDto>> GetAllAsync(
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Vehicles);
        }

        public Task<SavedVehicleDto?> GetByIdAsync(
            Guid userId,
            Guid vehicleId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(GetByIdResult);
        }

        public Task<SavedVehicleDto> CreateAsync(
            Guid userId,
            CreateSavedVehicleRequest request,
            CancellationToken cancellationToken = default)
        {
            if (ThrowValidationException)
            {
                throw new SavedVehicleValidationException(
                    ["Nickname is required."]);
            }

            return Task.FromResult(
                CreateResult
                ?? throw new InvalidOperationException(
                    "CreateResult was not configured."));
        }

        public Task<SavedVehicleDto?> UpdateAsync(
            Guid userId,
            Guid vehicleId,
            UpdateSavedVehicleRequest request,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<SavedVehicleDto?>(null);
        }

        public Task<bool> DeleteAsync(
            Guid userId,
            Guid vehicleId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(DeleteResult);
        }
    }
}