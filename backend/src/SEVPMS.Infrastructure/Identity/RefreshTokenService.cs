using System.Security.Cryptography;

namespace SEVPMS.Infrastructure.Identity;

public sealed class RefreshTokenService
{
    public string GenerateToken()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
    }
}
