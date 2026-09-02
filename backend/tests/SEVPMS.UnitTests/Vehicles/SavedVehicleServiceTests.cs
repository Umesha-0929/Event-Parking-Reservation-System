using SEVPMS.Application.Features.Vehicles.DTOs;
using SEVPMS.Application.Features.Vehicles.Interfaces;
using SEVPMS.Application.Features.Vehicles.Services;
using SEVPMS.Domain.Entities.Vehicles;
using Xunit;

namespace SEVPMS.UnitTests.Vehicles;

public sealed class SavedVehicleServiceTests
{
    [Fact]
    public async Task CreateAsync_WithValidRequest_AddsVehicleAndReturnsDto()
    {
        var repository = new FakeSavedVehicleRepository();
        var service = new SavedVehicleService(repository);
        var userId = Guid.NewGuid();

        var request = new CreateSavedVehicleRequest
        {
            Nickname = "  My Car  ",
            RegistrationNo = "  WP CAB 1234  ",
            VehicleType = "  Car  ",
            IsDefault = true
        };

        var result = await service.CreateAsync(userId, request);

        Assert.Equal(userId, result.UserId);
        Assert.Equal("My Car", result.Nickname);
        Assert.Equal("WP CAB 1234", result.RegistrationNo);
        Assert.Equal("Car", result.VehicleType);
        Assert.True(result.IsDefault);
        Assert.Single(repository.Vehicles);
        Assert.Equal(1, repository.SaveChangesCallCount);
    }

    [Fact]
    public async Task GetByIdAsync_WhenVehicleBelongsToAnotherUser_ReturnsNull()
    {
        var ownerId = Guid.NewGuid();
        var anotherUserId = Guid.NewGuid();

        var repository = new FakeSavedVehicleRepository();

        var vehicle = new SavedVehicle
        {
            UserId = ownerId,
            Nickname = "Owner Car",
            RegistrationNo = "WP ABC 1111",
            VehicleType = "Car"
        };

        repository.Vehicles.Add(vehicle);

        var service = new SavedVehicleService(repository);

        var result = await service.GetByIdAsync(
            anotherUserId,
            vehicle.Id);

        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateAsync_WhenVehicleBelongsToUser_UpdatesVehicle()
    {
        var userId = Guid.NewGuid();

        var repository = new FakeSavedVehicleRepository();

        var vehicle = new SavedVehicle
        {
            UserId = userId,
            Nickname = "Old Car",
            RegistrationNo = "WP OLD 1111",
            VehicleType = "Car",
            IsDefault = false
        };

        repository.Vehicles.Add(vehicle);

        var service = new SavedVehicleService(repository);

        var request = new UpdateSavedVehicleRequest
        {
            Nickname = "  Updated Car  ",
            RegistrationNo = "  WP NEW 2222  ",
            VehicleType = "  SUV  ",
            IsDefault = true
        };

        var result = await service.UpdateAsync(
            userId,
            vehicle.Id,
            request);

        Assert.NotNull(result);
        Assert.Equal("Updated Car", result.Nickname);
        Assert.Equal("WP NEW 2222", result.RegistrationNo);
        Assert.Equal("SUV", result.VehicleType);
        Assert.True(result.IsDefault);
        Assert.NotNull(vehicle.UpdatedAtUtc);
        Assert.Equal(1, repository.SaveChangesCallCount);
    }

    [Fact]
    public async Task UpdateAsync_WhenVehicleBelongsToAnotherUser_ReturnsNull()
    {
        var ownerId = Guid.NewGuid();
        var anotherUserId = Guid.NewGuid();

        var repository = new FakeSavedVehicleRepository();

        var vehicle = new SavedVehicle
        {
            UserId = ownerId,
            Nickname = "Owner Car",
            RegistrationNo = "WP ABC 1234",
            VehicleType = "Car"
        };

        repository.Vehicles.Add(vehicle);

        var service = new SavedVehicleService(repository);

        var request = new UpdateSavedVehicleRequest
        {
            Nickname = "Changed",
            RegistrationNo = "WP XYZ 9999",
            VehicleType = "SUV"
        };

        var result = await service.UpdateAsync(
            anotherUserId,
            vehicle.Id,
            request);

        Assert.Null(result);
        Assert.Equal("Owner Car", vehicle.Nickname);
        Assert.Equal(0, repository.SaveChangesCallCount);
    }

    [Fact]
    public async Task DeleteAsync_WhenVehicleBelongsToUser_RemovesVehicle()
    {
        var userId = Guid.NewGuid();

        var repository = new FakeSavedVehicleRepository();

        var vehicle = new SavedVehicle
        {
            UserId = userId,
            Nickname = "My Car",
            RegistrationNo = "WP CAB 5678",
            VehicleType = "Car"
        };

        repository.Vehicles.Add(vehicle);

        var service = new SavedVehicleService(repository);

        var result = await service.DeleteAsync(
            userId,
            vehicle.Id);

        Assert.True(result);
        Assert.Empty(repository.Vehicles);
        Assert.Equal(1, repository.SaveChangesCallCount);
    }

    [Fact]
    public async Task DeleteAsync_WhenVehicleBelongsToAnotherUser_DoesNotRemoveVehicle()
    {
        var ownerId = Guid.NewGuid();
        var anotherUserId = Guid.NewGuid();

        var repository = new FakeSavedVehicleRepository();

        var vehicle = new SavedVehicle
        {
            UserId = ownerId,
            Nickname = "Owner Car",
            RegistrationNo = "WP ABC 8888",
            VehicleType = "Car"
        };

        repository.Vehicles.Add(vehicle);

        var service = new SavedVehicleService(repository);

        var result = await service.DeleteAsync(
            anotherUserId,
            vehicle.Id);

        Assert.False(result);
        Assert.Single(repository.Vehicles);
        Assert.Equal(0, repository.SaveChangesCallCount);
    }

    private sealed class FakeSavedVehicleRepository
        : ISavedVehicleRepository
    {
        public List<SavedVehicle> Vehicles { get; } = [];

        public int SaveChangesCallCount { get; private set; }

        public Task<IReadOnlyList<SavedVehicle>> GetByUserIdAsync(
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            IReadOnlyList<SavedVehicle> result = Vehicles
                .Where(vehicle => vehicle.UserId == userId)
                .ToList();

            return Task.FromResult(result);
        }

        public Task<SavedVehicle?> GetByIdAsync(
            Guid vehicleId,
            CancellationToken cancellationToken = default)
        {
            var vehicle = Vehicles
                .SingleOrDefault(vehicle => vehicle.Id == vehicleId);

            return Task.FromResult(vehicle);
        }

        public Task AddAsync(
            SavedVehicle vehicle,
            CancellationToken cancellationToken = default)
        {
            Vehicles.Add(vehicle);

            return Task.CompletedTask;
        }

        public void Update(SavedVehicle vehicle)
        {
        }

        public void Remove(SavedVehicle vehicle)
        {
            Vehicles.Remove(vehicle);
        }

        public Task SaveChangesAsync(
            CancellationToken cancellationToken = default)
        {
            SaveChangesCallCount++;

            return Task.CompletedTask;
        }
    }
}