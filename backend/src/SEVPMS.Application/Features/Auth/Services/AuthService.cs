using SEVPMS.Application.Features.Auth.DTOs;
using SEVPMS.Application.Features.Auth.Interfaces;
using SEVPMS.Application.Interfaces.Repositories;
using SEVPMS.Domain.Entities.Users;
using SEVPMS.Domain.Enums;

namespace SEVPMS.Application.Features.Auth.Services;

public sealed class AuthService(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    IJwtTokenService jwtTokenService,
    IRefreshTokenService refreshTokenService)
    : IAuthService
{
    private const int MaxFailedLoginAttempts = 5;
    private const int LockoutMinutes = 15;

    public async Task<AuthResponse> RegisterAsync(
        RegisterRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        ValidateRegistration(request);

        var email = request.Email.Trim();

        var normalizedEmail =
            email.ToUpperInvariant();

        var existingUser =
            await userRepository
                .GetByNormalizedEmailAsync(
                    normalizedEmail,
                    cancellationToken);

        if (existingUser is not null)
        {
            throw new InvalidOperationException(
                "An account already exists with this email.");
        }

        var user = new User
        {
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Email = email,
            NormalizedEmail = normalizedEmail,
            PhoneNumber =
                string.IsNullOrWhiteSpace(request.PhoneNumber)
                    ? null
                    : request.PhoneNumber.Trim(),
            Role = request.Role,
            Status = AccountStatus.Active,
            PasswordHash =
                passwordHasher.HashPassword(
                    request.Password)
        };

        var refreshToken =
            refreshTokenService.GenerateToken();

        user.RefreshTokens.Add(
            new RefreshToken
            {
                TokenHash =
                    refreshToken.TokenHash,

                ExpiresAtUtc =
                    refreshToken.ExpiresAtUtc
            });

        await userRepository.AddAsync(
            user,
            cancellationToken);

        await userRepository.SaveChangesAsync(
            cancellationToken);

        return CreateAuthResponse(
            user,
            refreshToken);
    }

    public async Task<AuthResponse> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var normalizedEmail =
            request.Email
                .Trim()
                .ToUpperInvariant();

        var user =
            await userRepository
                .GetByNormalizedEmailAsync(
                    normalizedEmail,
                    cancellationToken);

        if (user is null)
        {
            throw new UnauthorizedAccessException(
                "Invalid email or password.");
        }

        var now = DateTime.UtcNow;

        if (user.LockoutEndUtc.HasValue &&
            user.LockoutEndUtc.Value > now)
        {
            throw new UnauthorizedAccessException(
                "Account is temporarily locked.");
        }

        if (user.Status != AccountStatus.Active)
        {
            throw new UnauthorizedAccessException(
                "Account is not active.");
        }

        var passwordValid =
            passwordHasher.VerifyPassword(
                user.PasswordHash,
                request.Password);

        if (!passwordValid)
        {
            user.FailedLoginAttempts++;

            if (user.FailedLoginAttempts >=
                MaxFailedLoginAttempts)
            {
                user.LockoutEndUtc =
                    now.AddMinutes(
                        LockoutMinutes);
            }

            await userRepository.SaveChangesAsync(
                cancellationToken);

            throw new UnauthorizedAccessException(
                "Invalid email or password.");
        }

        user.FailedLoginAttempts = 0;
        user.LockoutEndUtc = null;
        user.LastLoginAtUtc = now;

        var refreshToken =
            refreshTokenService.GenerateToken();

        user.RefreshTokens.Add(
            new RefreshToken
            {
                TokenHash =
                    refreshToken.TokenHash,

                ExpiresAtUtc =
                    refreshToken.ExpiresAtUtc
            });

        await userRepository.SaveChangesAsync(
            cancellationToken);

        return CreateAuthResponse(
            user,
            refreshToken);
    }

    public async Task<AuthResponse> RefreshTokenAsync(
        RefreshTokenRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(
            request.RefreshToken))
        {
            throw new UnauthorizedAccessException(
                "Invalid refresh token.");
        }

        var tokenHash =
            refreshTokenService.HashToken(
                request.RefreshToken);

        var storedToken =
            await userRepository
                .GetByRefreshTokenHashAsync(
                    tokenHash,
                    cancellationToken);

        if (storedToken is null ||
            storedToken.IsRevoked ||
            storedToken.IsExpired)
        {
            throw new UnauthorizedAccessException(
                "Invalid or expired refresh token.");
        }

        var user = storedToken.User;

        if (user.Status != AccountStatus.Active)
        {
            throw new UnauthorizedAccessException(
                "Account is not active.");
        }

        // Rotate the refresh token.
        storedToken.RevokedAtUtc =
            DateTime.UtcNow;

        var newRefreshToken =
            refreshTokenService.GenerateToken();

        user.RefreshTokens.Add(
            new RefreshToken
            {
                TokenHash =
                    newRefreshToken.TokenHash,

                ExpiresAtUtc =
                    newRefreshToken.ExpiresAtUtc
            });

        await userRepository.SaveChangesAsync(
            cancellationToken);

        return CreateAuthResponse(
            user,
            newRefreshToken);
    }

    private AuthResponse CreateAuthResponse(
        User user,
        RefreshTokenResult refreshToken)
    {
        var accessToken =
            jwtTokenService
                .GenerateAccessToken(user);

        return new AuthResponse
        {
            UserId = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Role = user.Role,

            AccessToken =
                accessToken.Token,

            AccessTokenExpiresAtUtc =
                accessToken.ExpiresAtUtc,

            RefreshToken =
                refreshToken.Token,

            RefreshTokenExpiresAtUtc =
                refreshToken.ExpiresAtUtc
        };
    }

    private static void ValidateRegistration(
        RegisterRequest request)
    {
        if (string.IsNullOrWhiteSpace(
            request.FirstName))
        {
            throw new ArgumentException(
                "First name is required.");
        }

        if (string.IsNullOrWhiteSpace(
            request.LastName))
        {
            throw new ArgumentException(
                "Last name is required.");
        }

        if (string.IsNullOrWhiteSpace(
            request.Email))
        {
            throw new ArgumentException(
                "Email is required.");
        }

        if (string.IsNullOrWhiteSpace(
            request.Password))
        {
            throw new ArgumentException(
                "Password is required.");
        }

        if (request.Password.Length < 8)
        {
            throw new ArgumentException(
                "Password must contain at least 8 characters.");
        }

        // Admin accounts must not be created
        // through public registration.
        if (request.Role == UserRole.Admin)
        {
            throw new InvalidOperationException(
                "Admin registration is not allowed.");
        }
    }
}