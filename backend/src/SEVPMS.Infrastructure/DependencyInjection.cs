using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SEVPMS.Application.Interfaces.Providers;
using SEVPMS.Infrastructure.Persistence;
using SEVPMS.Infrastructure.Providers.Email;
using SEVPMS.Infrastructure.Providers.Payments.MockPayment;
using SEVPMS.Infrastructure.Providers.Sms;

namespace SEVPMS.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString =
            configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' was not found.");

        services.AddDbContext<SEVPMSDbContext>(options =>
            options.UseSqlServer(connectionString));

        // Development-safe provider implementations.
        services.AddScoped<IPaymentProvider, MockPaymentProvider>();
        services.AddScoped<ISmsSender, ConsoleSmsSender>();
        services.AddScoped<IEmailSender, ConsoleEmailSender>();

        return services;
    }
}
