using SEVPMS.Application.Features.Auth.DTOs;
using SEVPMS.Domain.Entities.Users;

namespace SEVPMS.Application.Features.Auth.Interfaces;

public interface IJwtTokenService
{
    AccessTokenResult GenerateAccessToken(User user);
}