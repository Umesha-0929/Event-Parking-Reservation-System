using Microsoft.EntityFrameworkCore;
using SEVPMS.Domain.Entities.Users;
using SEVPMS.Domain.Entities.Venues;
using SEVPMS.Domain.Entities.Events;

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

    public DbSet<Event> Events => Set<Event>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(SEVPMSDbContext).Assembly);
    }
}