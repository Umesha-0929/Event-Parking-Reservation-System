using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SEVPMS.Application.Features.Users.DTOs;
using SEVPMS.Application.Features.Users.Interfaces;

namespace SEVPMS.Api.Controllers;

[ApiController]
[Route("api/users")]
[Authorize]
public sealed class UsersController(
    IUserService userService)
    : ControllerBase
{
    // =========================================================
    // GET CURRENT USER
    // =========================================================
    [HttpGet("me")]
    public async Task<ActionResult<UserProfileResponse>> GetMe(
        CancellationToken cancellationToken)
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

        var response =
            await userService.GetProfileAsync(
                userId,
                cancellationToken);

        return Ok(response);
    }

    // =========================================================
    // UPDATE CURRENT USER PROFILE
    // =========================================================
    [HttpPut("me")]
    public async Task<ActionResult<UserProfileResponse>>
        UpdateMe(
            [FromBody] UpdateProfileRequest request,
            CancellationToken cancellationToken)
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

        var response =
            await userService.UpdateProfileAsync(
                userId,
                request,
                cancellationToken);

        return Ok(response);
    }

    [HttpPut("me/password")]
    public async Task<IActionResult> ChangePassword(
        [FromBody] ChangePasswordRequest request,
        CancellationToken cancellationToken)
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

        await userService.ChangePasswordAsync(
            userId,
            request,
            cancellationToken);

        return NoContent();
    }
}