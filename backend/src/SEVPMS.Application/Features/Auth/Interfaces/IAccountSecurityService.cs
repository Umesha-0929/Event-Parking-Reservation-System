using SEVPMS.Application.Features.Auth.DTOs;

namespace SEVPMS.Application.Features.Auth.Interfaces;

public interface IAccountSecurityService
{
    Task LogoutAsync(Guid userId, LogoutRequest request, CancellationToken cancellationToken = default);
    Task RequestPasswordResetAsync(RequestPasswordResetRequest request, CancellationToken cancellationToken = default);
    Task ConfirmPasswordResetAsync(ConfirmPasswordResetRequest request, CancellationToken cancellationToken = default);
}
