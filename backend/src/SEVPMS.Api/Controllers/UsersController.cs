using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SEVPMS.Application.Features.Users.DTOs;
using SEVPMS.Domain.Enums;

namespace SEVPMS.Api.Controllers;

[ApiController]
[Route("api/users")]
[Authorize]
public sealed class UsersController : ControllerBase
{
    [HttpGet("me")]
    public ActionResult<UserProfileResponse> GetMe()
    {
        var userIdValue =
            User.FindFirstValue(
                ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(
            userIdValue,
            out var userId))
        {
            return Unauthorized();
        }

        var email =
            User.FindFirstValue(
                JwtRegisteredClaimNames.Email)
            ?? User.FindFirstValue(
                ClaimTypes.Email)
            ?? string.Empty;

        var name =
            User.FindFirstValue(
                ClaimTypes.Name)
            ?? string.Empty;

        var roleValue =
            User.FindFirstValue(
                ClaimTypes.Role);

        if (!Enum.TryParse<UserRole>(
            roleValue,
            true,
            out var role))
        {
            return Unauthorized();
        }

        return Ok(
            new UserProfileResponse
            {
                UserId = userId,
                Email = email,
                Name = name,
                Role = role
            });
    }
}