using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SEVPMS.Application.Features.Auth.DTOs;
using SEVPMS.Application.Features.Auth.Interfaces;
using SEVPMS.Domain.Entities.Users;

namespace SEVPMS.Infrastructure.Identity;

public sealed class JwtTokenService(
    IOptions<JwtOptions> options)
    : IJwtTokenService
{
    private readonly JwtOptions _options = options.Value;

    public AccessTokenResult GenerateAccessToken(User user)
    {
        if (string.IsNullOrWhiteSpace(_options.Key))
        {
            throw new InvalidOperationException(
                "JWT signing key is not configured.");
        }

        if (_options.AccessTokenMinutes <= 0)
        {
            throw new InvalidOperationException(
                "JWT access token lifetime is invalid.");
        }

        var now = DateTime.UtcNow;

        var expiresAtUtc =
            now.AddMinutes(_options.AccessTokenMinutes);

        var claims = new List<Claim>
        {
            new(
                JwtRegisteredClaimNames.Sub,
                user.Id.ToString()),

            new(
                JwtRegisteredClaimNames.Email,
                user.Email),

            new(
                ClaimTypes.NameIdentifier,
                user.Id.ToString()),

            new(
                ClaimTypes.Name,
                $"{user.FirstName} {user.LastName}".Trim()),

            new(
                ClaimTypes.Role,
                user.Role.ToString()),

            new(
                JwtRegisteredClaimNames.Jti,
                Guid.NewGuid().ToString("N"))
        };

        var securityKey =
            new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_options.Key));

        var credentials =
            new SigningCredentials(
                securityKey,
                SecurityAlgorithms.HmacSha256);

        var token =
            new JwtSecurityToken(
                issuer: _options.Issuer,
                audience: _options.Audience,
                claims: claims,
                notBefore: now,
                expires: expiresAtUtc,
                signingCredentials: credentials);

        var tokenValue =
            new JwtSecurityTokenHandler()
                .WriteToken(token);

        return new AccessTokenResult(
            tokenValue,
            expiresAtUtc);
    }
}