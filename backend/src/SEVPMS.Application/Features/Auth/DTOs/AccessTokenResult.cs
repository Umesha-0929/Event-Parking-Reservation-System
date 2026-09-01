namespace SEVPMS.Application.Features.Auth.DTOs;

public sealed record AccessTokenResult(
    string Token,
    DateTime ExpiresAtUtc);