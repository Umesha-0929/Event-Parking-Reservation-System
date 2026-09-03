using System.Security.Cryptography;
using System.Text;
using SEVPMS.Application.Features.Auth.DTOs;
using SEVPMS.Application.Features.Auth.Interfaces;
using SEVPMS.Application.Interfaces.Providers;
using SEVPMS.Application.Interfaces.Repositories;
using SEVPMS.Domain.Entities.Users;

namespace SEVPMS.Application.Features.Auth.Services;

public sealed class AccountSecurityService(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    IRefreshTokenService refreshTokenService,
    IEmailSender emailSender)
    : IAccountSecurityService
{
    private static readonly TimeSpan ResetLifetime = TimeSpan.FromMinutes(30);

    public async Task LogoutAsync(
        Guid userId,
        LogoutRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var now = DateTime.UtcNow;

        if (request.AllSessions || string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            await userRepository.RevokeActiveRefreshTokensAsync(userId, now, cancellationToken);
            await userRepository.SaveChangesAsync(cancellationToken);
            return;
        }

        var hash = refreshTokenService.HashToken(request.RefreshToken.Trim());
        var token = await userRepository.GetByRefreshTokenHashAsync(hash, cancellationToken);

        if (token is not null && token.UserId == userId && token.RevokedAtUtc is null)
        {
            token.RevokedAtUtc = now;
            await userRepository.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task RequestPasswordResetAsync(
        RequestPasswordResetRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(request.Email))
            return;

        var normalized = request.Email.Trim().ToUpperInvariant();
        var user = await userRepository.GetByNormalizedEmailAsync(normalized, cancellationToken);

        // Never reveal whether an account exists.
        if (user is null)
            return;

        var rawToken = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
        var hash = Hash(rawToken);
        var now = DateTime.UtcNow;

        await userRepository.InvalidatePasswordResetTokensAsync(user.Id, now, cancellationToken);

        await userRepository.AddPasswordResetTokenAsync(
            new PasswordResetToken
            {
                UserId = user.Id,
                TokenHash = hash,
                ExpiresAtUtc = now.Add(ResetLifetime),
                CreatedAtUtc = now
            },
            cancellationToken);

        await userRepository.SaveChangesAsync(cancellationToken);

        await emailSender.SendAsync(
            user.Email,
            "SEVPMS password reset",
            $"Use this one-time password reset token within 30 minutes: {rawToken}",
            cancellationToken);
    }

    public async Task ConfirmPasswordResetAsync(
        ConfirmPasswordResetRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(request.Token))
            throw new ArgumentException("Password reset token is required.");

        if (string.IsNullOrWhiteSpace(request.NewPassword) || request.NewPassword.Length < 8)
            throw new ArgumentException("New password must contain at least 8 characters.");

        var stored = await userRepository.GetPasswordResetTokenByHashAsync(
            Hash(request.Token.Trim()),
            cancellationToken);

        if (stored is null || stored.IsUsed || stored.IsExpired)
            throw new UnauthorizedAccessException("Password reset token is invalid or expired.");

        var user = stored.User;
        user.PasswordHash = passwordHasher.HashPassword(request.NewPassword);
        user.FailedLoginAttempts = 0;
        user.LockoutEndUtc = null;
        user.UpdatedAtUtc = DateTime.UtcNow;

        stored.UsedAtUtc = DateTime.UtcNow;

        await userRepository.RevokeActiveRefreshTokensAsync(
            user.Id,
            DateTime.UtcNow,
            cancellationToken);

        await userRepository.SaveChangesAsync(cancellationToken);
    }

    private static string Hash(string value)
        => Convert.ToHexString(
            SHA256.HashData(
                Encoding.UTF8.GetBytes(value)));
}
