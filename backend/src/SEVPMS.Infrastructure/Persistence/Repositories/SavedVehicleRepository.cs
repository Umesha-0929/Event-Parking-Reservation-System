using Microsoft.EntityFrameworkCore;
using SEVPMS.Application.Features.Vehicles.Interfaces;
using SEVPMS.Domain.Entities.Vehicles;

namespace SEVPMS.Infrastructure.Persistence.Repositories;

public sealed class SavedVehicleRepository(
    SEVPMSDbContext dbContext) : ISavedVehicleRepository
{
    public async Task<IReadOnlyList<SavedVehicle>> GetByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext
            .Set<SavedVehicle>()
            .Where(vehicle => vehicle.UserId == userId)
            .ToListAsync(cancellationToken);
    }

    public async Task<SavedVehicle?> GetByIdAsync(
        Guid vehicleId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext
            .Set<SavedVehicle>()
            .SingleOrDefaultAsync(
                vehicle => vehicle.Id == vehicleId,
                cancellationToken);
    }

    public async Task AddAsync(
        SavedVehicle vehicle,
        CancellationToken cancellationToken = default)
    {
        await dbContext
            .Set<SavedVehicle>()
            .AddAsync(vehicle, cancellationToken);
    }

    public void Update(SavedVehicle vehicle)
    {
        dbContext
            .Set<SavedVehicle>()
            .Update(vehicle);
    }

    public void Remove(SavedVehicle vehicle)
    {
        dbContext
            .Set<SavedVehicle>()
            .Remove(vehicle);
    }

    public async Task SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}