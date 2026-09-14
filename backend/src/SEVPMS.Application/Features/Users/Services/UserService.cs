using SEVPMS.Application.Features.Users.DTOs;
using SEVPMS.Application.Features.Users.Interfaces;
using SEVPMS.Application.Interfaces.Repositories;
using SEVPMS.Application.Features.Auth.Interfaces;

namespace SEVPMS.Application.Features.Users.Services;

public sealed class UserService(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher)
    : IUserService
{   
    public async Task<UserProfileResponse> GetProfileAsync(
    Guid userId,
    CancellationToken cancellationToken = default)
        {
            var user =
             await userRepository.GetByIdAsync(
                userId,
                cancellationToken);

        if (user is null)
        {
            throw new InvalidOperationException(
                "User account was not found.");
        }

        return new UserProfileResponse
        {
            UserId = user.Id,
            Email = user.Email,
            Name = $"{user.FirstName} {user.LastName}",
            PhoneNumber = user.PhoneNumber,
            Role = user.Role
        };
    }

    public async Task<UserProfileResponse> UpdateProfileAsync(
        Guid userId,
        UpdateProfileRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(request.FirstName))
        {
            throw new ArgumentException(
                "First name is required.");
        }

        if (string.IsNullOrWhiteSpace(request.LastName))
        {
            throw new ArgumentException(
                "Last name is required.");
        }

        var user =
            await userRepository.GetByIdAsync(
                userId,
                cancellationToken);

        if (user is null)
        {
            throw new InvalidOperationException(
                "User account was not found.");
        }

        user.FirstName =
            request.FirstName.Trim();

        user.LastName =
            request.LastName.Trim();

        user.PhoneNumber =
            string.IsNullOrWhiteSpace(request.PhoneNumber)
                ? null
                : request.PhoneNumber.Trim();

        user.UpdatedAtUtc =
            DateTime.UtcNow;

        await userRepository.SaveChangesAsync(
            cancellationToken);

        return new UserProfileResponse
        {
            UserId = user.Id,
            Email = user.Email,
            Name =
                $"{user.FirstName} {user.LastName}",
            PhoneNumber = user.PhoneNumber,
            Role = user.Role
        };
    }

    public async Task ChangePasswordAsync(
        Guid userId,
        ChangePasswordRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(
            request.CurrentPassword))
        {
            throw new ArgumentException(
                "Current password is required.");
        }

        if (string.IsNullOrWhiteSpace(
            request.NewPassword))
        {
            throw new ArgumentException(
                "New password is required.");
        }

        if (request.NewPassword.Length < 8)
        {
            throw new ArgumentException(
                "New password must be at least 8 characters.");
        }

        if (request.CurrentPassword ==
            request.NewPassword)
        {
            throw new ArgumentException(
                "New password must be different from current password.");
        }

        var user =
            await userRepository.GetByIdAsync(
                userId,
                cancellationToken);

        if (user is null)
        {
            throw new InvalidOperationException(
                "User account was not found.");
        }

        var isCurrentPasswordValid =
            passwordHasher.VerifyPassword(
            user.PasswordHash,
            request.CurrentPassword);

        if (!isCurrentPasswordValid)
        {
            throw new UnauthorizedAccessException(
                "Current password is incorrect.");
        }

        var changedAtUtc =
            DateTime.UtcNow;

        user.PasswordHash =
            passwordHasher.HashPassword(
                request.NewPassword);

        user.UpdatedAtUtc =
            changedAtUtc;

        await userRepository
            .RevokeActiveRefreshTokensAsync(
                userId,
                changedAtUtc,
                cancellationToken);

        await userRepository.SaveChangesAsync(
            cancellationToken);
    }
}