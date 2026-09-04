using Microsoft.Extensions.DependencyInjection;
using SEVPMS.Application.Features.Reviews.Interfaces;
using SEVPMS.Application.Features.Reviews.Services;
using SEVPMS.Infrastructure.Persistence.Repositories;

namespace SEVPMS.Infrastructure;

public static class ReviewModuleDependencyInjection
{
    public static IServiceCollection AddReviewModule(
        this IServiceCollection services)
    {
        services.AddScoped<
            IEventReviewRepository,
            EventReviewRepository>();

        services.AddScoped<
            IEventReviewService,
            EventReviewService>();

        return services;
    }
}