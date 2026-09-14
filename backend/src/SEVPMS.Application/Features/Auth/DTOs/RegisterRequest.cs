using SEVPMS.Domain.Enums;

namespace SEVPMS.Application.Features.Auth.DTOs;

public sealed class RegisterRequest
{
    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public string? PhoneNumber { get; set; }

    public UserRole Role { get; set; } = UserRole.Customer;
}