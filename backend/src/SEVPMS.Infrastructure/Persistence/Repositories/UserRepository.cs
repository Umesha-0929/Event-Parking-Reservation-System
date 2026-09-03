using Microsoft.EntityFrameworkCore;
using SEVPMS.Application.Interfaces.Repositories;
using SEVPMS.Domain.Entities.Users;

namespace SEVPMS.Infrastructure.Persistence.Repositories;

public sealed class UserRepository(SEVPMSDbContext dbContext) : IUserRepository
{
    public async Task<IReadOnlyList<User>> GetAllAsync(CancellationToken cancellationToken = default)
        => await dbContext.Users.AsNoTracking()
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToListAsync(cancellationToken);

    public Task<User?> GetByIdAsync(Guid userId, CancellationToken cancellationToken = default)
        => dbContext.Users.FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);

    public Task<User?> GetByNormalizedEmailAsync(string normalizedEmail, CancellationToken cancellationToken = default)
        => dbContext.Users.FirstOrDefaultAsync(x => x.NormalizedEmail == normalizedEmail, cancellationToken);

    public Task<RefreshToken?> GetByRefreshTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default)
        => dbContext.RefreshTokens.Include(x => x.User)
            .FirstOrDefaultAsync(x => x.TokenHash == tokenHash, cancellationToken);

    public Task<PasswordResetToken?> GetPasswordResetTokenByHashAsync(string tokenHash, CancellationToken cancellationToken = default)
        => dbContext.PasswordResetTokens.Include(x => x.User)
            .FirstOrDefaultAsync(x => x.TokenHash == tokenHash, cancellationToken);

    public async Task AddAsync(User user, CancellationToken cancellationToken = default)
        => await dbContext.Users.AddAsync(user, cancellationToken);

    public async Task AddRefreshTokenAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default)
        => await dbContext.RefreshTokens.AddAsync(refreshToken, cancellationToken);

    public async Task AddPasswordResetTokenAsync(PasswordResetToken token, CancellationToken cancellationToken = default)
        => await dbContext.PasswordResetTokens.AddAsync(token, cancellationToken);

    public async Task RevokeActiveRefreshTokensAsync(
        Guid userId,
        DateTime revokedAtUtc,
        CancellationToken cancellationToken = default)
    {
        var tokens = await dbContext.RefreshTokens
            .Where(x => x.UserId == userId &&
                        x.RevokedAtUtc == null &&
                        x.ExpiresAtUtc > revokedAtUtc)
            .ToListAsync(cancellationToken);

        foreach (var token in tokens)
            token.RevokedAtUtc = revokedAtUtc;
    }

    public async Task InvalidatePasswordResetTokensAsync(
        Guid userId,
        DateTime usedAtUtc,
        CancellationToken cancellationToken = default)
    {
        var tokens = await dbContext.PasswordResetTokens
            .Where(x => x.UserId == userId &&
                        x.UsedAtUtc == null &&
                        x.ExpiresAtUtc > usedAtUtc)
            .ToListAsync(cancellationToken);

        foreach (var token in tokens)
            token.UsedAtUtc = usedAtUtc;
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        => dbContext.SaveChangesAsync(cancellationToken);
}
