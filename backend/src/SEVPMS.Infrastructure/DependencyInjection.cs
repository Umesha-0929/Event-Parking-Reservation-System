using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SEVPMS.Application.Features.Auth.Interfaces;
using SEVPMS.Application.Features.Auth.Services;
using SEVPMS.Application.Features.Bookings.Interfaces;
using SEVPMS.Application.Features.Bookings.Services;
using SEVPMS.Application.Features.Events.Interfaces;
using SEVPMS.Application.Features.Events.Services;
using SEVPMS.Application.Features.Notifications.Interfaces;
using SEVPMS.Application.Features.Notifications.Services;
using SEVPMS.Application.Features.Payments.Interfaces;
using SEVPMS.Application.Features.Payments.Services;
using SEVPMS.Application.Features.Receipts.Interfaces;
using SEVPMS.Application.Features.Receipts.Services;
using SEVPMS.Application.Features.Users.Interfaces;
using SEVPMS.Application.Features.Users.Services;
using SEVPMS.Application.Features.Venues.Interfaces;
using SEVPMS.Application.Features.Venues.Services;
using SEVPMS.Application.Features.VenueRentals.Interfaces;
using SEVPMS.Application.Features.VenueRentals.Services;
using SEVPMS.Application.Interfaces.Providers;
using SEVPMS.Application.Interfaces.Repositories;
using SEVPMS.Infrastructure.Identity;
using SEVPMS.Infrastructure.Persistence;
using SEVPMS.Infrastructure.Persistence.Repositories;
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
            ?? throw new InvalidOperationException(
                "Connection string 'DefaultConnection' was not found.");

        services.AddDbContext<SEVPMSDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.Configure<JwtOptions>(
            configuration.GetSection(JwtOptions.SectionName));

        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddSingleton<IJwtTokenService, JwtTokenService>();
        services.AddSingleton<IRefreshTokenService, RefreshTokenService>();

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IVenueRepository, VenueRepository>();
        services.AddScoped<IEventRepository, EventRepository>();
        services.AddScoped<IVenueRentalRepository, VenueRentalRepository>();
        services.AddScoped<IBookingRepository, BookingRepository>();
        services.AddScoped<IPaymentRepository, PaymentRepository>();
        services.AddScoped<IReceiptRepository, ReceiptRepository>();
        services.AddScoped<INotificationRepository, NotificationRepository>();

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IAdminUserService, AdminUserService>();
        services.AddScoped<IVenueService, VenueService>();
        services.AddScoped<IEventService, EventService>();
        services.AddScoped<IVenueRentalService, VenueRentalService>();
        services.AddScoped<IBookingService, BookingService>();
        services.AddScoped<IPaymentService, PaymentService>();
        services.AddScoped<IReceiptService, ReceiptService>();
        services.AddScoped<INotificationService, NotificationService>();

        services.AddScoped<IPaymentProvider, MockPaymentProvider>();
        services.AddScoped<ISmsSender, ConsoleSmsSender>();
        services.AddScoped<IEmailSender, ConsoleEmailSender>();

        services.AddVehicleModule();
        services.AddParkingModule();
        services.AddFoodModule();

        return services;
    }
}