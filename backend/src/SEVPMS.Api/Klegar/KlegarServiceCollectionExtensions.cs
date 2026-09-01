using SEVPMS.Application.Features.Seats.Interfaces;
using SEVPMS.Application.Features.Seats.Services;
using SEVPMS.Application.Features.Tickets.Interfaces;
using SEVPMS.Application.Features.Tickets.Services;
using SEVPMS.Infrastructure.Persistence.Repositories.Seats;
using SEVPMS.Infrastructure.Persistence.Repositories.Tickets;
using SEVPMS.Infrastructure.Security;
namespace SEVPMS.Api.Klegar;
public static class KlegarServiceCollectionExtensions
{
    public static IServiceCollection AddKlegarBackend(this IServiceCollection services)
    {
        services.AddSingleton(TimeProvider.System);
        services.AddScoped<ISeatInventoryRepository, EfSeatInventoryRepository>();
        services.AddScoped<ISeatingLayoutRepository, EfSeatingLayoutRepository>(); services.AddScoped<ITicketRepository, EfTicketRepository>();
        services.AddScoped<ISeatService, SeatService>();
        services.AddScoped<ISeatingLayoutService, SeatingLayoutService>(); services.AddScoped<ITicketService, TicketService>();
        services.AddSingleton<ITicketQrTokenService, HmacTicketQrTokenService>();
        services.AddScoped<KlegarRealtimeNotifier>(); services.AddScoped<ISeatRealtimeNotifier>(sp => sp.GetRequiredService<KlegarRealtimeNotifier>()); services.AddScoped<ITicketRealtimeNotifier>(sp => sp.GetRequiredService<KlegarRealtimeNotifier>());
        services.AddScoped<RequestUserResolver>();
        return services;
    }
}

