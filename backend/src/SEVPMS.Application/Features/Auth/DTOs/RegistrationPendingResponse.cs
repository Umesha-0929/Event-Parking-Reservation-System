namespace SEVPMS.Application.Features.Auth.DTOs;

public sealed class RegistrationPendingResponse
{
    public string Email { get; set; } = string.Empty;

    public bool EmailVerificationRequired { get; set; } = true;

    public DateTime OtpExpiresAtUtc { get; set; }

    public string Message { get; set; } =
        "Registration successful. Please verify your email.";
}