using SEVPMS.Application.Features.Vehicles.DTOs;

namespace SEVPMS.Application.Features.Vehicles.Interfaces;

public interface ISavedVehicleService
{
    Task<IReadOnlyList<SavedVehicleDto>> GetAllAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<SavedVehicleDto?> GetByIdAsync(
        Guid userId,
        Guid vehicleId,
        CancellationToken cancellationToken = default);

    Task<SavedVehicleDto> CreateAsync(
        Guid userId,
        CreateSavedVehicleRequest request,
        CancellationToken cancellationToken = default);

    Task<SavedVehicleDto?> UpdateAsync(
        Guid userId,
        Guid vehicleId,
        UpdateSavedVehicleRequest request,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(
        Guid userId,
        Guid vehicleId,
        CancellationToken cancellationToken = default);
}