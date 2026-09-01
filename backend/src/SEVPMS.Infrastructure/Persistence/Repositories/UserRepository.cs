using Microsoft.EntityFrameworkCore;
using SEVPMS.Application.Interfaces.Repositories;
using SEVPMS.Domain.Entities.Users;

namespace SEVPMS.Infrastructure.Persistence.Repositories;

public sealed class UserRepository(
    SEVPMSDbContext dbContext)
    : IUserRepository
{
    public Task<User?> GetByIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return dbContext.Users
            .FirstOrDefaultAsync(
                x => x.Id == userId,
                cancellationToken);
    }
    
    public Task<User?> GetByNormalizedEmailAsync(
        string normalizedEmail,
        CancellationToken cancellationToken = default)
    {
        return dbContext.Users
            .FirstOrDefaultAsync(
                x => x.NormalizedEmail == normalizedEmail,
                cancellationToken);
    }

    public Task<RefreshToken?> GetByRefreshTokenHashAsync(
        string tokenHash,
        CancellationToken cancellationToken = default)
    {
        return dbContext.RefreshTokens
            .Include(x => x.User)
            .FirstOrDefaultAsync(
                x => x.TokenHash == tokenHash,
                cancellationToken);
    }

    public async Task AddAsync(
        User user,
        CancellationToken cancellationToken = default)
    {
        await dbContext.Users.AddAsync(
            user,
            cancellationToken);
    }

    public async Task SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        await dbContext.SaveChangesAsync(
            cancellationToken);
    }

    public async Task AddRefreshTokenAsync(
        RefreshToken refreshToken,
        CancellationToken cancellationToken = default)
    {
        await dbContext.RefreshTokens.AddAsync(
            refreshToken,
            cancellationToken);
    }
}