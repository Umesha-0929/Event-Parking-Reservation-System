namespace SEVPMS.Application.Features.Auth.DTOs;

public sealed class EmailVerificationResponse
{
    public bool Verified { get; set; }

    public string Message { get; set; } = string.Empty;
}