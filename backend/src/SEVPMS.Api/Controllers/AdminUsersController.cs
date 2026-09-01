using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SEVPMS.Api.Authorization;
using SEVPMS.Application.Features.Users.DTOs;
using SEVPMS.Application.Features.Users.Interfaces;

namespace SEVPMS.Api.Controllers;

[ApiController]
[Route("api/admin/users")]
[Authorize(Policy = AuthorizationPolicies.AdminOnly)]
public sealed class AdminUsersController(
    IAdminUserService adminUserService)
    : ControllerBase
{
    // =========================================================
    // GET ALL USERS
    // GET /api/admin/users
    // =========================================================
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<AdminUserResponse>>>
        GetAllUsers(
            CancellationToken cancellationToken)
    {
        var users =
            await adminUserService.GetAllUsersAsync(
                cancellationToken);

        return Ok(users);
    }

    // =========================================================
    // GET USER BY ID
    // GET /api/admin/users/{id}
    // =========================================================
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<AdminUserResponse>>
        GetUserById(
            Guid id,
            CancellationToken cancellationToken)
    {
        var user =
            await adminUserService.GetUserByIdAsync(
                id,
                cancellationToken);

        return Ok(user);
    }

    // =========================================================
    // UPDATE USER STATUS
    // PUT /api/admin/users/{id}/status
    // =========================================================
    [HttpPut("{id:guid}/status")]
    public async Task<ActionResult<AdminUserResponse>>
        UpdateUserStatus(
            Guid id,
            [FromBody] UpdateUserStatusRequest request,
            CancellationToken cancellationToken)
    {
        var user =
            await adminUserService.UpdateUserStatusAsync(
                id,
                request,
                cancellationToken);

        return Ok(user);
    }
}