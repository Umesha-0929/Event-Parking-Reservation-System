using SEVPMS.Application.Features.Auth.DTOs;

namespace SEVPMS.Application.Features.Auth.Interfaces;

public interface IRefreshTokenService
{
    RefreshTokenResult GenerateToken();

    string HashToken(string token);
}