using SEVPMS.Application.Features.Users.Interfaces;
using SEVPMS.Application.Features.Users.Services;
using SEVPMS.Application.Features.Auth.Services;
using SEVPMS.Application.Interfaces.Repositories;
using SEVPMS.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SEVPMS.Application.Interfaces.Providers;
using SEVPMS.Infrastructure.Persistence;
using SEVPMS.Infrastructure.Providers.Email;
using SEVPMS.Infrastructure.Providers.Payments.MockPayment;
using SEVPMS.Infrastructure.Providers.Sms;
using SEVPMS.Application.Features.Auth.Interfaces;
using SEVPMS.Infrastructure.Identity;


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

        services.Configure<JwtOptions>(
            configuration.GetSection(JwtOptions.SectionName));

        services.AddSingleton<IPasswordHasher, PasswordHasher>();

        services.AddSingleton<IJwtTokenService, JwtTokenService>();

        services.AddScoped<IUserRepository, UserRepository>();

        services.AddSingleton<IRefreshTokenService, RefreshTokenService>();

        services.AddScoped<IAuthService, AuthService>();

        services.AddScoped<IUserService, UserService>();

        services.AddScoped<IAdminUserService, AdminUserService>();
        // Development-safe provider implementations.
        services.AddScoped<IPaymentProvider, MockPaymentProvider>();
        services.AddScoped<ISmsSender, ConsoleSmsSender>();
        services.AddScoped<IEmailSender, ConsoleEmailSender>();

        return services;
    }
}
