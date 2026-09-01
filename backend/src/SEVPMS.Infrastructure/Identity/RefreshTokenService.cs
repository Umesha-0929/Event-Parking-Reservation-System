using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.WebUtilities;
using SEVPMS.Application.Features.Auth.DTOs;
using SEVPMS.Application.Features.Auth.Interfaces;

namespace SEVPMS.Infrastructure.Identity;

public sealed class RefreshTokenService
    : IRefreshTokenService
{
    private const int RefreshTokenLifetimeDays = 7;

    public RefreshTokenResult GenerateToken()
    {
        var token =
            WebEncoders.Base64UrlEncode(
                RandomNumberGenerator.GetBytes(64));

        var tokenHash = HashToken(token);

        var expiresAtUtc =
            DateTime.UtcNow.AddDays(
                RefreshTokenLifetimeDays);

        return new RefreshTokenResult(
            token,
            tokenHash,
            expiresAtUtc);
    }

    public string HashToken(string token)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(token);

        var bytes =
            Encoding.UTF8.GetBytes(token);

        var hash =
            SHA256.HashData(bytes);

        return Convert.ToHexString(hash);
    }
}