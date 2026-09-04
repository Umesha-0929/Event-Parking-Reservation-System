using Microsoft.Extensions.DependencyInjection;
using SEVPMS.Application.Features.Recommendations.Interfaces;
using SEVPMS.Application.Features.Recommendations.Services;

namespace SEVPMS.Infrastructure;

public static class RecommendationModuleDependencyInjection
{
    public static IServiceCollection AddRecommendationModule(
        this IServiceCollection services)
    {
        services.AddScoped<
            IEventRecommendationService,
            EventRecommendationService>();

        return services;
    }
}