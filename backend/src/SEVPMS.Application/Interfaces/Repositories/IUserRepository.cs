using SEVPMS.Domain.Entities.Users;

namespace SEVPMS.Application.Interfaces.Repositories;

public interface IUserRepository
{
    Task<IReadOnlyList<User>> GetAllAsync(CancellationToken cancellationToken = default);
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
