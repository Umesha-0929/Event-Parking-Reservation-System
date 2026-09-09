using SEVPMS.Domain.Entities.Users;

namespace SEVPMS.Application.Interfaces.Repositories;

public interface IUserRepository
{
    Task<IReadOnlyList<User>> GetAllAsync(CancellationToken cancellationToken = default);
    async Task<IReadOnlyList<User>> GetPageAsync(
        int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var all = await GetAllAsync(cancellationToken);
        var safePage = Math.Max(page, 1);
        var safePageSize = Math.Clamp(pageSize, 1, 200);
        return all.Skip((safePage - 1) * safePageSize).Take(safePageSize).ToArray();
    }
    Task<User?> GetByIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<User?> GetByNormalizedEmailAsync(string normalizedEmail, CancellationToken cancellationToken = default);
    Task<RefreshToken?> GetByRefreshTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default);
    Task<PasswordResetToken?> GetPasswordResetTokenByHashAsync(string tokenHash, CancellationToken cancellationToken = default);
    Task AddAsync(User user, CancellationToken cancellationToken = default);
    Task AddRefreshTokenAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default);
    Task AddPasswordResetTokenAsync(PasswordResetToken token, CancellationToken cancellationToken = default);
    Task RevokeActiveRefreshTokensAsync(Guid userId, DateTime revokedAtUtc, CancellationToken cancellationToken = default);
    Task InvalidatePasswordResetTokensAsync(Guid userId, DateTime usedAtUtc, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
