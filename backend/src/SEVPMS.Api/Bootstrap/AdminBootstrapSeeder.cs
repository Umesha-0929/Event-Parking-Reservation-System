using Microsoft.EntityFrameworkCore;
using SEVPMS.Application.Features.Auth.Interfaces;
using SEVPMS.Domain.Entities.Users;
using SEVPMS.Domain.Enums;
using SEVPMS.Infrastructure.Persistence;

namespace SEVPMS.Api.Bootstrap;

public static class AdminBootstrapSeeder
{
    public static async Task SeedAsync(IServiceProvider services, IConfiguration configuration)
    {
        using var scope = services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<SEVPMSDbContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        var email = configuration["AdminBootstrap:Email"];
        var password = configuration["AdminBootstrap:Password"];
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password)) return;

        var normalizedEmail = email.Trim().ToUpperInvariant();
        var existing = await dbContext.Users.FirstOrDefaultAsync(x => x.NormalizedEmail == normalizedEmail);

        // Bootstrap creates a development admin only when one does not exist.
        // Existing credentials are never silently reset during application startup.
        if (existing is not null) return;

        var admin = new User
        {
            FirstName = "System", LastName = "Admin", Email = email.Trim(), NormalizedEmail = normalizedEmail,
            Role = UserRole.Admin, Status = AccountStatus.Active,
            PasswordHash = passwordHasher.HashPassword(password), EmailVerifiedAtUtc = DateTime.UtcNow
        };
        await dbContext.Users.AddAsync(admin);
        await dbContext.SaveChangesAsync();
    }
}
