using SEVPMS.Application.Features.Users.DTOs;

namespace SEVPMS.Application.Features.Users.Interfaces;

public interface IUserService
{
    Task<UserProfileResponse> GetProfileAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<UserProfileResponse> UpdateProfileAsync(
        Guid userId,
        UpdateProfileRequest request,
        CancellationToken cancellationToken = default);
}