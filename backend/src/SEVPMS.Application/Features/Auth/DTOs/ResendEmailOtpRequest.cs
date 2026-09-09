namespace SEVPMS.Application.Features.Auth.DTOs;

public sealed class ResendEmailOtpRequest
{
    public string Email { get; set; } = string.Empty;
}