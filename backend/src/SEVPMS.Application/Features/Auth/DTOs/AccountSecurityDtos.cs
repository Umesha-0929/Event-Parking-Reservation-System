namespace SEVPMS.Application.Features.Auth.DTOs;

public sealed class LogoutRequest
{
    public string? RefreshToken { get; set; }
    public bool AllSessions { get; set; } = true;
}

public sealed class RequestPasswordResetRequest
{
    public string Email { get; set; } = string.Empty;
}

public sealed class ConfirmPasswordResetRequest
{
    public string Token { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}
