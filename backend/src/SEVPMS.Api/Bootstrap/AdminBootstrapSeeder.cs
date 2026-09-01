using Microsoft.EntityFrameworkCore;
using SEVPMS.Application.Features.Auth.Interfaces;
using SEVPMS.Domain.Entities.Users;
using SEVPMS.Domain.Enums;
using SEVPMS.Infrastructure.Persistence;

namespace SEVPMS.Api.Bootstrap;

public static class AdminBootstrapSeeder
{
    public static async Task SeedAsync(
        IServiceProvider services,
        IConfiguration configuration)
    {
        using var scope = services.CreateScope();

        var dbContext =
            scope.ServiceProvider
                .GetRequiredService<SEVPMSDbContext>();

        var passwordHasher =
            scope.ServiceProvider
                .GetRequiredService<IPasswordHasher>();

        var email =
            configuration["AdminBootstrap:Email"];

        var password =
            configuration["AdminBootstrap:Password"];

        if (string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(password))
        {
            return;
        }

        var normalizedEmail =
            email.Trim().ToUpperInvariant();

        var existingAdmin =
            await dbContext.Users
                .FirstOrDefaultAsync(
                    x => x.NormalizedEmail ==
                         normalizedEmail);

        // Existing admin-na current development
        // secret password-oda update pannuvom.
        if (existingAdmin is not null)
        {
            existingAdmin.FirstName = "System";
            existingAdmin.LastName = "Admin";

            existingAdmin.Email =
                email.Trim();

            existingAdmin.NormalizedEmail =
                normalizedEmail;

            existingAdmin.Role =
                UserRole.Admin;

            existingAdmin.Status =
                AccountStatus.Active;

            existingAdmin.FailedLoginAttempts = 0;
            existingAdmin.LockoutEndUtc = null;

            existingAdmin.PasswordHash =
                passwordHasher.HashPassword(
                    password);

            await dbContext.SaveChangesAsync();

            return;
        }

        var admin =
            new User
            {
                FirstName = "System",
                LastName = "Admin",

                Email = email.Trim(),

                NormalizedEmail =
                    normalizedEmail,

                Role = UserRole.Admin,

                Status = AccountStatus.Active,

                PasswordHash =
                    passwordHasher.HashPassword(
                        password)
            };

        await dbContext.Users.AddAsync(admin);

        await dbContext.SaveChangesAsync();
    }
}