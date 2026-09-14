using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SEVPMS.Api.Authorization;
using SEVPMS.Application.Features.Users.DTOs;
using SEVPMS.Application.Features.Users.Interfaces;
using System.Security.Claims;
using SEVPMS.Application.Features.Audit.Interfaces;
using SEVPMS.Domain.Enums;

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
            [FromQuery] int page,
            [FromQuery] int pageSize,
            CancellationToken cancellationToken)
    {
        var users =
            await adminUserService.GetUsersPageAsync(
                page == 0 ? 1 : page,
                pageSize == 0 ? 50 : pageSize,
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
        var actorUserId = CurrentActorUserId();
        if (actorUserId == id && request.Status != AccountStatus.Active)
            return BadRequest(new { message = "You cannot suspend or deactivate your own signed-in admin account." });

        var before =
            await adminUserService.GetUserByIdAsync(
                id,
                cancellationToken);

        var user =
            await adminUserService.UpdateUserStatusAsync(
                id,
                request,
                cancellationToken);

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


    // =========================================================
    // PERMANENTLY DELETE USER ACCOUNT
    // DELETE /api/admin/users/{id}
    // =========================================================
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteUserPermanently(
        Guid id,
        CancellationToken cancellationToken)
    {
        var actorUserId = CurrentActorUserId();
        if (actorUserId == id)
            return BadRequest(new { message = "You cannot permanently delete your own signed-in admin account." });

        var before = await adminUserService.GetUserByIdAsync(id, cancellationToken);
        await adminUserService.DeleteUserPermanentlyAsync(id, cancellationToken);

        await auditLogService.WriteAsync(
            actorUserId,
            "Admin permanently deleted user account",
            "User",
            id.ToString(),
            $"{before.Email} | {before.Role} | {before.Status}",
            "Deleted",
            HttpContext.TraceIdentifier,
            HttpContext.Connection.RemoteIpAddress?.ToString(),
            cancellationToken);

        return NoContent();
    }

    private Guid? CurrentActorUserId()
    {
        var rawActor = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(rawActor, out var parsed) ? parsed : null;
    }
}