using Microsoft.AspNetCore.Identity;
using SEVPMS.Application.Features.Auth.Interfaces;

namespace SEVPMS.Infrastructure.Identity;

public sealed class PasswordHasher : IPasswordHasher
{
    private readonly Microsoft.AspNetCore.Identity.PasswordHasher<object>
        _hasher = new();

    private readonly object _user = new();

    public string HashPassword(string password)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(password);

        return _hasher.HashPassword(
            _user,
            password);
    }

    public bool VerifyPassword(
        string passwordHash,
        string providedPassword)
    {
        if (string.IsNullOrWhiteSpace(passwordHash) ||
            string.IsNullOrWhiteSpace(providedPassword))
        {
            return false;
        }

        var result = _hasher.VerifyHashedPassword(
            _user,
            passwordHash,
            providedPassword);

        return result != PasswordVerificationResult.Failed;
    }
}