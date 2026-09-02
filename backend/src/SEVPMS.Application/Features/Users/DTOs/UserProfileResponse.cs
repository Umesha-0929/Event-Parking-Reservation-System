using SEVPMS.Domain.Enums;

namespace SEVPMS.Application.Features.Users.DTOs;

public sealed class UserProfileResponse
{
    public Guid UserId { get; set; }

    public string Email { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? PhoneNumber { get; set; }

    public UserRole Role { get; set; }
}