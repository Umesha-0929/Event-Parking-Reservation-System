using SEVPMS.Domain.Entities.Vehicles;

namespace SEVPMS.Application.Features.Vehicles.Interfaces;

public interface ISavedVehicleRepository
{
    Task<IReadOnlyList<SavedVehicle>> GetByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<SavedVehicle?> GetByIdAsync(
        Guid vehicleId,
        CancellationToken cancellationToken = default);

    Task<SavedVehicle?> GetByRegistrationNoAsync(
        Guid userId,
        string registrationNo,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        SavedVehicle vehicle,
        CancellationToken cancellationToken = default);

    void Update(SavedVehicle vehicle);

    void Remove(SavedVehicle vehicle);

    Task SaveChangesAsync(
        CancellationToken cancellationToken = default);
}