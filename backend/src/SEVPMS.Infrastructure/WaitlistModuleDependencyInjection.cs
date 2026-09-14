using Microsoft.Extensions.DependencyInjection;
using SEVPMS.Application.Features.Waitlists.Interfaces;
using SEVPMS.Application.Features.Waitlists.Services;
using SEVPMS.Infrastructure.Persistence.Repositories;

namespace SEVPMS.Infrastructure;

public static class WaitlistModuleDependencyInjection
{
    public static IServiceCollection AddWaitlistModule(
        this IServiceCollection services)
    {
        services.AddScoped<
            IWaitlistRepository,
            WaitlistRepository>();

        services.AddScoped<
            IWaitlistService,
            WaitlistService>();

        return services;
    }
}
