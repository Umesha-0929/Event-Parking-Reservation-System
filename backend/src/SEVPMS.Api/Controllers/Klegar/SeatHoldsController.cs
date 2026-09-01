using Microsoft.AspNetCore.Mvc;
using SEVPMS.Api.Klegar;
using SEVPMS.Application.Features.Seats.DTOs;
using SEVPMS.Application.Features.Seats.Interfaces;
namespace SEVPMS.Api.Controllers.Klegar;
[ApiController]
public sealed class SeatHoldsController(ISeatService seats, RequestUserResolver users) : ControllerBase
{
    [HttpPost("api/events/{eventId:guid}/seat-holds")]
    public async Task<ActionResult<SeatHoldResponse>> Hold(Guid eventId, [FromBody] CreateSeatHoldRequest request, CancellationToken ct)
    {
        if (!users.TryGetUserId(HttpContext, out var userId)) return Unauthorized(new { error = "user_required" });
        var result = await seats.HoldAsync(eventId, userId, request, ct); return result.Succeeded ? Ok(result) : Conflict(result);
    }
    [HttpDelete("api/seat-holds/{holdToken}")]
    public async Task<IActionResult> Release(string holdToken, CancellationToken ct)
    {
        if (!users.TryGetUserId(HttpContext, out var userId)) return Unauthorized(new { error = "user_required" });
        return await seats.ReleaseHoldAsync(holdToken, userId, ct) ? NoContent() : NotFound();
    }
    [HttpPost("api/seat-holds/{holdToken}/commit")]
    public async Task<IActionResult> Commit(string holdToken, [FromQuery] Guid bookingId, [FromQuery] Guid customerUserId, CancellationToken ct)
    {
        if (!users.IsOrganizerOrAdmin(HttpContext)) return Forbid();
        return await seats.CommitHoldAsync(holdToken, customerUserId, bookingId, ct) ? Ok(new { committed = true, bookingId }) : Conflict(new { committed = false, error = "hold_invalid_or_expired" });
    }
}
