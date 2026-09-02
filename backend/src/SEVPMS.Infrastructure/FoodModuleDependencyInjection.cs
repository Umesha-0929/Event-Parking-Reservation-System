using Microsoft.Extensions.DependencyInjection;
using SEVPMS.Application.Features.Food.Interfaces;
using SEVPMS.Application.Features.Food.Services;
using SEVPMS.Infrastructure.Persistence.Repositories;

namespace SEVPMS.Infrastructure;

public static class FoodModuleDependencyInjection
{
    public static IServiceCollection AddFoodModule(
        this IServiceCollection services)
    {
        services.AddScoped<IFoodRepository, FoodRepository>();
        services.AddScoped<IFoodService, FoodService>();

        return services;
    }
}