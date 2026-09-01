using Microsoft.Extensions.DependencyInjection;
using SEVPMS.Realtime.Dispatchers;

namespace SEVPMS.Realtime;

public static class DependencyInjection
{
    public static IServiceCollection AddRealtime(this IServiceCollection services)
    {
        services.AddSignalR();
        services.AddScoped<IRealtimeDispatcher, SignalRRealtimeDispatcher>();
        return services;
    }
}
