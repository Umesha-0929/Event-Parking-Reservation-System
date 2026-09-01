using SEVPMS.Application.Features.Users.DTOs;

namespace SEVPMS.Application.Features.Users.Interfaces;

public interface IAdminUserService
{
    Task<IReadOnlyList<AdminUserResponse>> GetAllUsersAsync(
        CancellationToken cancellationToken = default);

    Task<AdminUserResponse> GetUserByIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<AdminUserResponse> UpdateUserStatusAsync(
        Guid userId,
        UpdateUserStatusRequest request,
        CancellationToken cancellationToken = default);
}