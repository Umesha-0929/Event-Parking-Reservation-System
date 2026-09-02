using SEVPMS.Application.Features.Users.DTOs;
using SEVPMS.Application.Features.Users.Interfaces;
using SEVPMS.Application.Interfaces.Repositories;

namespace SEVPMS.Application.Features.Users.Services;

public sealed class AdminUserService(
    IUserRepository userRepository)
    : IAdminUserService
{
    public async Task<IReadOnlyList<AdminUserResponse>>
        GetAllUsersAsync(
            CancellationToken cancellationToken = default)
    {
        var users =
            await userRepository.GetAllAsync(
                cancellationToken);

        return users
            .Select(
                user => new AdminUserResponse
                {
                    UserId = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    Role = user.Role,
                    Status = user.Status,
                    CreatedAtUtc = user.CreatedAtUtc,
                    LastLoginAtUtc = user.LastLoginAtUtc
                })
            .ToList();
    }

    public async Task<AdminUserResponse> GetUserByIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var user =
            await userRepository.GetByIdAsync(
                userId,
                cancellationToken);

        if (user is null)
        {
            throw new KeyNotFoundException(
                "User account was not found.");
        }

        return new AdminUserResponse
        {
            UserId = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            Role = user.Role,
            Status = user.Status,
            CreatedAtUtc = user.CreatedAtUtc,
            LastLoginAtUtc = user.LastLoginAtUtc
        };
    }

    public async Task<AdminUserResponse> UpdateUserStatusAsync(
        Guid userId,
        UpdateUserStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (!Enum.IsDefined(request.Status))
        {
            throw new ArgumentException(
                "Invalid account status.");
        }

        var user =
            await userRepository.GetByIdAsync(
                userId,
                cancellationToken);

        if (user is null)
        {
            throw new KeyNotFoundException(
                "User account was not found.");
        }

        user.Status = request.Status;
        user.UpdatedAtUtc = DateTime.UtcNow;

        await userRepository.SaveChangesAsync(
            cancellationToken);

        return new AdminUserResponse
        {
            UserId = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            Role = user.Role,
            Status = user.Status,
            CreatedAtUtc = user.CreatedAtUtc,
            LastLoginAtUtc = user.LastLoginAtUtc
        };
    }
}