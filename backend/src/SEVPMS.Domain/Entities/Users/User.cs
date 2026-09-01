using SEVPMS.Domain.Common;
using SEVPMS.Domain.Enums;

namespace SEVPMS.Domain.Entities.Users;

public sealed class User : AuditableEntity
{
    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string NormalizedEmail { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public string? PhoneNumber { get; set; }

    public UserRole Role { get; set; } = UserRole.Customer;

    public AccountStatus Status { get; set; } = AccountStatus.Active;

    public int FailedLoginAttempts { get; set; }

    public DateTime? LockoutEndUtc { get; set; }

    public DateTime? LastLoginAtUtc { get; set; }

    public ICollection<RefreshToken> RefreshTokens { get; set; }
    = new List<RefreshToken>();

    public ICollection<PasswordResetToken> PasswordResetTokens { get; set; }
    = new List<PasswordResetToken>();
}