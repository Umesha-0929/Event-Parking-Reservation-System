using SEVPMS.Application.Features.Users.DTOs;

namespace SEVPMS.Application.Features.Users.Interfaces;

public interface IAdminUserService
{
    Task<IReadOnlyList<AdminUserResponse>> GetAllUsersAsync(
        CancellationToken cancellationToken = default);

    async Task<IReadOnlyList<AdminUserResponse>> GetUsersPageAsync(
        int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var all = await GetAllUsersAsync(cancellationToken);
        var safePage = Math.Max(page, 1);
        var safePageSize = Math.Clamp(pageSize, 1, 200);
        return all.Skip((safePage - 1) * safePageSize).Take(safePageSize).ToArray();
    }

    Task<AdminUserResponse> GetUserByIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<AdminUserResponse> UpdateUserStatusAsync(
        Guid userId,
        UpdateUserStatusRequest request,
        CancellationToken cancellationToken = default);
}