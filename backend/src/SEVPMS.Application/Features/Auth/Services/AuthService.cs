using SEVPMS.Application.Common.Exceptions;
using SEVPMS.Application.Features.Auth.DTOs;
using SEVPMS.Application.Features.Auth.Interfaces;
using SEVPMS.Application.Interfaces.Repositories;
using SEVPMS.Domain.Entities.Users;
using SEVPMS.Domain.Enums;
using System.Security.Cryptography;
using SEVPMS.Application.Interfaces.Providers;

namespace SEVPMS.Application.Features.Auth.Services;

public sealed class AuthService(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    IJwtTokenService jwtTokenService,
    IRefreshTokenService refreshTokenService,
    IEmailSender emailSender)
    : IAuthService
{
    private const int MaxFailedLoginAttempts = 5;
    private const int LockoutMinutes = 15;

    // =========================================================
    // REGISTER
    // =========================================================
    public async Task<RegistrationPendingResponse> RegisterAsync(
        RegisterRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        ValidateRegistration(request);

        var email = request.Email.Trim();
        var normalizedEmail = email.ToUpperInvariant();

        var existingUser =
            await userRepository.GetByNormalizedEmailAsync(
                normalizedEmail,
                cancellationToken);

        if (existingUser is not null)
        {
            throw new InvalidOperationException(
                "An account already exists with this email.");
        }

        var otp = GenerateEmailOtp();
        var now = DateTime.UtcNow;
        var otpExpiry = now.AddMinutes(5);

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
                passwordHasher.HashPassword(request.Password),

            EmailVerifiedAtUtc = null,

            EmailVerificationOtpHash =
                passwordHasher.HashPassword(otp),

            EmailVerificationOtpExpiresAtUtc = otpExpiry,
            EmailVerificationOtpSentAtUtc = now,
            EmailVerificationFailedAttempts = 0
        };

        await userRepository.AddAsync(
            user,
            cancellationToken);

        await userRepository.SaveChangesAsync(
            cancellationToken);

        await SendVerificationOtpAsync(
            user.Email,
            otp,
            cancellationToken);

        return new RegistrationPendingResponse
        {
            Email = user.Email,
            EmailVerificationRequired = true,
            OtpExpiresAtUtc = otpExpiry,
            Message =
                "Registration successful. Please verify your email using the OTP sent to you."
        };
    }

    public async Task<EmailVerificationResponse> VerifyEmailOtpAsync(
        VerifyEmailOtpRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(request.Email) ||
            string.IsNullOrWhiteSpace(request.Otp))
        {
            throw new ArgumentException(
                "Email and OTP are required.");
        }

        var normalizedEmail =
            request.Email.Trim().ToUpperInvariant();

        var user =
            await userRepository.GetByNormalizedEmailAsync(
                normalizedEmail,
                cancellationToken);

        if (user is null)
        {
            throw new UnauthorizedAccessException(
                "Invalid email or OTP.");
        }

        if (user.EmailVerifiedAtUtc.HasValue)
        {
            return new EmailVerificationResponse
            {
                Verified = true,
                Message = "Email is already verified."
            };
        }

        if (user.EmailVerificationFailedAttempts >= 5)
        {
            throw new UnauthorizedAccessException(
                "Too many invalid OTP attempts. Please request a new OTP.");
        }

        if (user.EmailVerificationOtpExpiresAtUtc is null ||
            user.EmailVerificationOtpExpiresAtUtc <= DateTime.UtcNow)
        {
            throw new UnauthorizedAccessException(
                "OTP has expired. Please request a new OTP.");
        }

        if (string.IsNullOrWhiteSpace(
                user.EmailVerificationOtpHash))
        {
            throw new UnauthorizedAccessException(
                "Invalid email or OTP.");
        }

        var validOtp =
            passwordHasher.VerifyPassword(
                user.EmailVerificationOtpHash,
                request.Otp.Trim());

        if (!validOtp)
        {
            user.EmailVerificationFailedAttempts++;

            await userRepository.SaveChangesAsync(
                cancellationToken);

            throw new UnauthorizedAccessException(
                "Invalid email or OTP.");
        }

        user.EmailVerifiedAtUtc = DateTime.UtcNow;

        user.EmailVerificationOtpHash = null;
        user.EmailVerificationOtpExpiresAtUtc = null;
        user.EmailVerificationOtpSentAtUtc = null;
        user.EmailVerificationFailedAttempts = 0;

        await userRepository.SaveChangesAsync(
            cancellationToken);

        return new EmailVerificationResponse
        {
            Verified = true,
            Message =
                "Email verified successfully. You can now sign in."
        };
    }

    public async Task<RegistrationPendingResponse> ResendEmailOtpAsync(
        ResendEmailOtpRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(request.Email))
        {
            throw new ArgumentException(
                "Email is required.");
        }

        var normalizedEmail =
            request.Email.Trim().ToUpperInvariant();

        var user =
            await userRepository.GetByNormalizedEmailAsync(
                normalizedEmail,
                cancellationToken);

        if (user is null)
        {
            throw new InvalidOperationException(
                "Account was not found.");
        }

        if (user.EmailVerifiedAtUtc.HasValue)
        {
            throw new InvalidOperationException(
                "Email is already verified.");
        }

        var now = DateTime.UtcNow;

        if (user.EmailVerificationOtpSentAtUtc.HasValue &&
            user.EmailVerificationOtpSentAtUtc.Value
                .AddSeconds(60) > now)
        {
            throw new InvalidOperationException(
                "Please wait before requesting another OTP.");
        }

        var otp = GenerateEmailOtp();
        var expiry = now.AddMinutes(5);

        user.EmailVerificationOtpHash =
            passwordHasher.HashPassword(otp);

        user.EmailVerificationOtpExpiresAtUtc = expiry;
        user.EmailVerificationOtpSentAtUtc = now;
        user.EmailVerificationFailedAttempts = 0;

        await userRepository.SaveChangesAsync(
            cancellationToken);

        await SendVerificationOtpAsync(
            user.Email,
            otp,
            cancellationToken);

        return new RegistrationPendingResponse
        {
            Email = user.Email,
            EmailVerificationRequired = true,
            OtpExpiresAtUtc = expiry,
            Message = "A new verification OTP has been sent."
        };
    }

    // =========================================================
    // LOGIN
    // =========================================================
    public async Task<AuthResponse> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(request.Email) ||
            string.IsNullOrWhiteSpace(request.Password))
        {
            throw new UnauthorizedAccessException(
                "Invalid email or password.");
        }

        var normalizedEmail =
            request.Email
                .Trim()
                .ToUpperInvariant();

        var user =
            await userRepository.GetByNormalizedEmailAsync(
                normalizedEmail,
                cancellationToken);

        if (user is null)
        {
            throw new UnauthorizedAccessException(
                "Invalid email or password.");
        }

        var now = DateTime.UtcNow;

        // Account lock check
        if (user.LockoutEndUtc.HasValue &&
            user.LockoutEndUtc.Value > now)
        {
            throw new UnauthorizedAccessException(
                "Account is temporarily locked.");
        }

        // Account status check
        if (user.Status != AccountStatus.Active)
        {
            throw new ForbiddenAccessException(
                "Account is not active.");
        }

        // Password verification
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

        if (!user.EmailVerifiedAtUtc.HasValue)
        {
            throw new ForbiddenAccessException(
            "Please verify your email before signing in.");
        }

        // Successful login
        user.FailedLoginAttempts = 0;
        user.LockoutEndUtc = null;
        user.LastLoginAtUtc = now;

        var refreshToken =
            refreshTokenService.GenerateToken();

        // IMPORTANT:
        // Existing user-ku navigation collection-la direct add
        // panna EF tracking issue varalaam.
        // So explicit repository Add use pannrom.
        var refreshTokenEntity =
            new RefreshToken
            {
                UserId = user.Id,
                TokenHash = refreshToken.TokenHash,
                ExpiresAtUtc = refreshToken.ExpiresAtUtc
            };

        await userRepository.AddRefreshTokenAsync(
            refreshTokenEntity,
            cancellationToken);

        await userRepository.SaveChangesAsync(
            cancellationToken);

        return CreateAuthResponse(
            user,
            refreshToken);
    }

    // =========================================================
    // REFRESH TOKEN
    // =========================================================
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

        // Browser sends raw refresh token.
        // DB stores only the hash.
        var tokenHash =
            refreshTokenService.HashToken(
                request.RefreshToken);

        var storedToken =
            await userRepository.GetByRefreshTokenHashAsync(
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
            throw new ForbiddenAccessException(
                "Account is not active.");
        }

        // Revoke old refresh token
        storedToken.RevokedAtUtc =
            DateTime.UtcNow;

        // Generate replacement refresh token
        var newRefreshToken =
            refreshTokenService.GenerateToken();

        var newRefreshTokenEntity =
            new RefreshToken
            {
                UserId = user.Id,
                TokenHash =
                    newRefreshToken.TokenHash,

                ExpiresAtUtc =
                    newRefreshToken.ExpiresAtUtc
            };

        await userRepository.AddRefreshTokenAsync(
            newRefreshTokenEntity,
            cancellationToken);

        await userRepository.SaveChangesAsync(
            cancellationToken);

        return CreateAuthResponse(
            user,
            newRefreshToken);
    }

    // =========================================================
    // CREATE AUTH RESPONSE
    // =========================================================
    private AuthResponse CreateAuthResponse(
        User user,
        RefreshTokenResult refreshToken)
    {
        var accessToken =
            jwtTokenService.GenerateAccessToken(
                user);

        return new AuthResponse
        {
            UserId = user.Id,

            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,

            Role = user.Role,

            TokenType = "Bearer",

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

    // =========================================================
    // REGISTER VALIDATION
    // =========================================================
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

        // Admin account public registration-la
        // create panna allow panna koodadhu.
        if (request.Role == UserRole.Admin)
        {
            throw new InvalidOperationException(
                "Admin registration is not allowed.");
        }
    }

    private static string GenerateEmailOtp()
    {
        return RandomNumberGenerator
            .GetInt32(100000, 1000000)
            .ToString();
    }

    private Task SendVerificationOtpAsync(
        string email,
        string otp,
        CancellationToken cancellationToken)
    {
        var subject = "Verify your Nvent email";

        var body =
            $"""
            Welcome to Nvent!

            Your email verification code is:

            {otp}

            This code will expire in 5 minutes.

            If you did not create this account, you can ignore this email.
            """;

        return emailSender.SendAsync(
            email,
            subject,
            body,
            cancellationToken);
    }
}