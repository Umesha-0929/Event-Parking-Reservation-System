namespace SEVPMS.Application.Features.Users.DTOs;

public sealed class UpdateProfileRequest
{
    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string? PhoneNumber { get; set; }
}