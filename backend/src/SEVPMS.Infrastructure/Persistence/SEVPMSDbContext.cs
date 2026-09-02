using Microsoft.EntityFrameworkCore;
using SEVPMS.Domain.Entities.Users;
using SEVPMS.Domain.Entities.Venues;

namespace SEVPMS.Infrastructure.Persistence;

public sealed class SEVPMSDbContext(
    DbContextOptions<SEVPMSDbContext> options)
    : DbContext(options)
{
    public DbSet<User> Users => Set<User>();

    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    public DbSet<PasswordResetToken> PasswordResetTokens
        => Set<PasswordResetToken>();
    
    public DbSet<Venue> Venues => Set<Venue>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(SEVPMSDbContext).Assembly);
    }
}