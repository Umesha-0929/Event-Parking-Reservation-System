using Microsoft.EntityFrameworkCore;
using SEVPMS.Domain.Entities.Users;

namespace SEVPMS.Infrastructure.Persistence;

public sealed class SEVPMSDbContext(
    DbContextOptions<SEVPMSDbContext> options)
    : DbContext(options)
{
    public DbSet<User> Users => Set<User>();

    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    public DbSet<PasswordResetToken> PasswordResetTokens
        => Set<PasswordResetToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(SEVPMSDbContext).Assembly);
    }
}