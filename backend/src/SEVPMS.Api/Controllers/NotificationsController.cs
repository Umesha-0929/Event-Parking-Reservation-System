using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SEVPMS.Application.Features.Notifications.DTOs;
using SEVPMS.Application.Features.Notifications.Interfaces;

namespace SEVPMS.Api.Controllers;

[ApiController]
[Route("api/notifications")]
[Authorize]
public sealed class NotificationsController(INotificationService notificationService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<NotificationResponse>>> GetMine(
        [FromQuery] int page,
        [FromQuery] int pageSize,
        CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var userId)) return Unauthorized();
        return Ok(await notificationService.GetMinePageAsync(
            userId,
            page == 0 ? 1 : page,
            pageSize == 0 ? 50 : pageSize,
            cancellationToken));
    }

    [HttpPut("{id:guid}/read")]
    public async Task<ActionResult<NotificationResponse>> MarkRead(Guid id, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var userId)) return Unauthorized();
        return Ok(await notificationService.MarkReadAsync(userId, id, cancellationToken));
    }

    [HttpPut("read-all")]
    public async Task<ActionResult<object>> MarkAllRead(CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var userId)) return Unauthorized();
        var updated = await notificationService.MarkAllReadAsync(userId, cancellationToken);
        return Ok(new { updated });
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> ClearOne(Guid id, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var userId)) return Unauthorized();
        await notificationService.DeleteAsync(userId, id, cancellationToken);
        return NoContent();
    }

    [HttpDelete]
    public async Task<ActionResult<object>> ClearAll(CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var userId)) return Unauthorized();
        var cleared = await notificationService.ClearAsync(userId, cancellationToken);
        return Ok(new { cleared });
    }

    private bool TryGetCurrentUserId(out Guid userId)
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(value, out userId);
    }
}
