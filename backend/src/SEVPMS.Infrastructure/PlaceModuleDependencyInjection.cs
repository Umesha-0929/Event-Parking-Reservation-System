using Microsoft.Extensions.DependencyInjection;
using SEVPMS.Application.Features.Places.Interfaces;
using SEVPMS.Application.Features.Places.Services;
using SEVPMS.Infrastructure.Persistence.Repositories;

namespace SEVPMS.Infrastructure;

public static class PlaceModuleDependencyInjection
{
    public static IServiceCollection AddPlaceModule(this IServiceCollection services)
    {
        services.AddScoped<IPlaceRepository, PlaceRepository>();
        services.AddScoped<IPlaceFinderService, PlaceFinderService>();
        return services;
    }
}
