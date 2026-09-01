using SEVPMS.Application.Features.Vehicles.DTOs;
using SEVPMS.Application.Features.Vehicles.Interfaces;
using SEVPMS.Domain.Entities.Vehicles;

namespace SEVPMS.Application.Features.Vehicles.Services;

public sealed class SavedVehicleService(
    ISavedVehicleRepository repository) : ISavedVehicleService
{
    public async Task<IReadOnlyList<SavedVehicleDto>> GetAllAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var vehicles = await repository.GetByUserIdAsync(
            userId,
            cancellationToken);

        return vehicles
            .Select(ToDto)
            .ToList();
    }

    public async Task<SavedVehicleDto?> GetByIdAsync(
        Guid userId,
        Guid vehicleId,
        CancellationToken cancellationToken = default)
    {
        var vehicle = await repository.GetByIdAsync(
            vehicleId,
            cancellationToken);

        if (vehicle is null || vehicle.UserId != userId)
        {
            return null;
        }

        return ToDto(vehicle);
    }

    public async Task<SavedVehicleDto> CreateAsync(
        Guid userId,
        CreateSavedVehicleRequest request,
        CancellationToken cancellationToken = default)
    {
        var vehicle = new SavedVehicle
        {
            UserId = userId,
            Nickname = request.Nickname.Trim(),
            RegistrationNo = request.RegistrationNo.Trim(),
            VehicleType = request.VehicleType.Trim(),
            IsDefault = request.IsDefault
        };

        await repository.AddAsync(vehicle, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        return ToDto(vehicle);
    }

    public async Task<SavedVehicleDto?> UpdateAsync(
        Guid userId,
        Guid vehicleId,
        UpdateSavedVehicleRequest request,
        CancellationToken cancellationToken = default)
    {
        var vehicle = await repository.GetByIdAsync(
            vehicleId,
            cancellationToken);

        if (vehicle is null || vehicle.UserId != userId)
        {
            return null;
        }

        vehicle.Nickname = request.Nickname.Trim();
        vehicle.RegistrationNo = request.RegistrationNo.Trim();
        vehicle.VehicleType = request.VehicleType.Trim();
        vehicle.IsDefault = request.IsDefault;
        vehicle.UpdatedAtUtc = DateTime.UtcNow;

        repository.Update(vehicle);
        await repository.SaveChangesAsync(cancellationToken);

        return ToDto(vehicle);
    }

    public async Task<bool> DeleteAsync(
        Guid userId,
        Guid vehicleId,
        CancellationToken cancellationToken = default)
    {
        var vehicle = await repository.GetByIdAsync(
            vehicleId,
            cancellationToken);

        if (vehicle is null || vehicle.UserId != userId)
        {
            return false;
        }

        repository.Remove(vehicle);
        await repository.SaveChangesAsync(cancellationToken);

        return true;
    }

    private static SavedVehicleDto ToDto(SavedVehicle vehicle)
    {
        return new SavedVehicleDto
        {
            Id = vehicle.Id,
            UserId = vehicle.UserId,
            Nickname = vehicle.Nickname,
            RegistrationNo = vehicle.RegistrationNo,
            VehicleType = vehicle.VehicleType,
            IsDefault = vehicle.IsDefault
        };
    }
}