using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SEVPMS.Api.Authorization;
using SEVPMS.Application.Features.Users.DTOs;
using SEVPMS.Application.Features.Users.Interfaces;
using System.Security.Claims;
using SEVPMS.Application.Features.Audit.Interfaces;

namespace SEVPMS.Api.Controllers;

[ApiController]
[Route("api/admin/users")]
[Authorize(Policy = AuthorizationPolicies.AdminOnly)]
public sealed class AdminUsersController(
    IAdminUserService adminUserService,
    IAuditLogService auditLogService)
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
        var before =
            await adminUserService.GetUserByIdAsync(
                id,
                cancellationToken);

        var user =
            await adminUserService.UpdateUserStatusAsync(
                id,
                request,
                cancellationToken);

        var rawActor =
            User.FindFirstValue(
                ClaimTypes.NameIdentifier);

        Guid? actorUserId =
            Guid.TryParse(rawActor, out var parsed)
                ? parsed
                : null;

        await auditLogService.WriteAsync(
            actorUserId,
            "Admin changed user account status",
            "User",
            id.ToString(),
            before.Status.ToString(),
            user.Status.ToString(),
            HttpContext.TraceIdentifier,
            HttpContext.Connection.RemoteIpAddress?.ToString(),
            cancellationToken);

        return Ok(user);
    }
}