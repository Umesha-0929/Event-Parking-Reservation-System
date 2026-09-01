using Microsoft.Extensions.DependencyInjection;
using SEVPMS.Application.Features.Parking.Interfaces;
using SEVPMS.Application.Features.Parking.Services;
using SEVPMS.Infrastructure.Persistence.Providers;
using SEVPMS.Infrastructure.Persistence.Repositories;

namespace SEVPMS.Infrastructure;

public static class ParkingModuleDependencyInjection
{
    public static IServiceCollection AddParkingModule(
        this IServiceCollection services)
    {
        services.AddScoped<IParkingRepository, ParkingRepository>();
        services.AddScoped<IParkingService, ParkingService>();

        services.AddScoped<IParkingRouteRepository, ParkingRouteRepository>();
        services.AddScoped<IParkingRouteService, ParkingRouteService>();

        services.AddScoped<
            IParkingRecommendationService,
            ParkingRecommendationService>();

        services.AddScoped<
            IParkingRecommendationCandidateProvider,
            ParkingRecommendationCandidateProvider>();

        return services;
    }
}