using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SEVPMS.Application.Features.Admin.Interfaces;
using SEVPMS.Application.Features.Admin.Services;
using SEVPMS.Application.Features.Audit.Interfaces;
using SEVPMS.Application.Features.Audit.Services;
using SEVPMS.Application.Features.Auth.Interfaces;
using SEVPMS.Application.Features.Auth.Services;
using SEVPMS.Application.Features.Bookings.Interfaces;
using SEVPMS.Application.Features.Bookings.Services;
using SEVPMS.Application.Features.EventCategories.Interfaces;
using SEVPMS.Application.Features.EventCategories.Services;
using SEVPMS.Application.Features.Events.Interfaces;
using SEVPMS.Application.Features.Events.Services;
using SEVPMS.Application.Features.Notifications.Interfaces;
using SEVPMS.Application.Features.Notifications.Services;
using SEVPMS.Application.Features.Payments.Interfaces;
using SEVPMS.Application.Features.Payments.Services;
using SEVPMS.Application.Features.Receipts.Interfaces;
using SEVPMS.Application.Features.Receipts.Services;
using SEVPMS.Application.Features.Reports.Interfaces;
using SEVPMS.Application.Features.Reports.Services;
using SEVPMS.Application.Features.Users.Interfaces;
using SEVPMS.Application.Features.Users.Services;
using SEVPMS.Application.Features.VenueMarketplace.Interfaces;
using SEVPMS.Application.Features.VenueMarketplace.Services;
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
using SEVPMS.Infrastructure.Providers.Payments;
using SEVPMS.Infrastructure.Providers.Payments.MockPayment;
using SEVPMS.Infrastructure.Providers.Sms;
using SEVPMS.Application.Features.Weather.Interfaces;
using SEVPMS.Application.Features.Weather.Services;
using SEVPMS.Infrastructure.Providers.Weather;
using SEVPMS.Application.Features.Calendar.Interfaces;
using SEVPMS.Application.Features.Calendar.Services;

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
        services.AddScoped<IEventCategoryRepository, EventCategoryRepository>();
        services.AddScoped<IVenueRentalRepository, VenueRentalRepository>();
        services.AddScoped<IVenueMarketplaceRepository, VenueMarketplaceRepository>();
        services.AddScoped<IBookingRepository, BookingRepository>();
        services.AddScoped<IPaymentRepository, PaymentRepository>();
        services.AddScoped<IPaymentTransactionRepository, PaymentTransactionRepository>();
        services.AddScoped<IRefundRepository, RefundRepository>();
        services.AddScoped<IReceiptRepository, ReceiptRepository>();
        services.AddScoped<IReceiptDeliveryRepository, ReceiptDeliveryRepository>();
        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<IAdminDashboardRepository, AdminDashboardRepository>();
        services.AddScoped<IAuditLogRepository, AuditLogRepository>();
        services.AddScoped<IReportRepository, ReportRepository>();

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IAccountSecurityService, AccountSecurityService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IAdminUserService, AdminUserService>();
        services.AddScoped<IVenueService, VenueService>();
        services.AddScoped<IVenueMarketplaceService, VenueMarketplaceService>();
        services.AddScoped<IEventCategoryService, EventCategoryService>();
        services.AddScoped<IEventService, EventService>();
        services.AddScoped<IVenueRentalService, VenueRentalService>();
        services.AddScoped<IBookingService, BookingService>();

        services.AddScoped<
            IConfirmedBookingCancellationService,
            ConfirmedBookingCancellationService>();

        services.AddScoped<IPaymentService, PaymentService>();
        services.AddScoped<IReceiptDeliveryService, ReceiptDeliveryService>();
        services.AddScoped<IReceiptService, ReceiptService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IAdminDashboardService, AdminDashboardService>();
        services.AddScoped<IAuditLogService, AuditLogService>();
        services.AddScoped<IReportService, ReportService>();

        services.AddScoped<IWeatherService, WeatherService>();
        services.AddScoped<IBookingCalendarService, BookingCalendarService>();
        services.AddHttpClient<IWeatherProvider, OpenMeteoWeatherProvider>(
            client =>
            {
                client.Timeout =
                    TimeSpan.FromSeconds(10);
            });

        services.AddScoped<IPaymentProvider, MockPaymentProvider>();

        services.AddScoped<
            ISandboxPaymentCallbackVerifier,
            SandboxPaymentCallbackVerifier>();

        services.AddScoped<IPayHereGatewayService, PayHereGatewayService>();

        services.AddScoped<ISmsSender>(sp =>
        {
            var cfg =
                sp.GetRequiredService<IConfiguration>();

            return string.IsNullOrWhiteSpace(
                cfg["Sms:Http:Endpoint"])
                ? new ConsoleSmsSender()
                : new HttpSmsSender(cfg);
        });

        services.AddScoped<IEmailSender>(sp =>
        {
            var cfg =
                sp.GetRequiredService<IConfiguration>();

            return string.IsNullOrWhiteSpace(
                cfg["Email:Smtp:Host"])
                ? new ConsoleEmailSender()
                : new SmtpEmailSender(cfg);
        });

        services.AddVehicleModule();
        services.AddParkingModule();
        services.AddFoodModule();
        services.AddPlaceModule();
        services.AddWaitlistModule();
        services.AddReviewModule();
        services.AddRecommendationModule();

        return services;
    }
}