namespace SEVPMS.Application.Features.Auth.DTOs;

public sealed class RefreshTokenRequest
{
    public string RefreshToken { get; set; } = string.Empty;
}