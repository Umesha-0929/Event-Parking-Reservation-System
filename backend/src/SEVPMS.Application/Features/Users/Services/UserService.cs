using SEVPMS.Application.Features.Users.DTOs;
using SEVPMS.Application.Features.Users.Interfaces;
using SEVPMS.Application.Interfaces.Repositories;

namespace SEVPMS.Application.Features.Users.Services;

public sealed class UserService(
    IUserRepository userRepository)
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
}