namespace SEVPMS.Application.Features.Auth.DTOs;

public sealed record RefreshTokenResult(
    string Token,
    string TokenHash,
    DateTime ExpiresAtUtc);