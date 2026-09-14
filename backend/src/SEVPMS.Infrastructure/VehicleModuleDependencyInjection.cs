using Microsoft.Extensions.DependencyInjection;
using SEVPMS.Application.Features.Vehicles.Interfaces;
using SEVPMS.Application.Features.Vehicles.Services;
using SEVPMS.Infrastructure.Persistence.Repositories;

namespace SEVPMS.Infrastructure;

public static class VehicleModuleDependencyInjection
{
    public static IServiceCollection AddVehicleModule(
        this IServiceCollection services)
    {
        services.AddScoped<ISavedVehicleRepository, SavedVehicleRepository>();
        services.AddScoped<ISavedVehicleService, SavedVehicleService>();

        return services;
    }
}