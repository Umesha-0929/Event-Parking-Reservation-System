using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SEVPMS.Api.Klegar;
using SEVPMS.Application.Features.Seats.DTOs;
using SEVPMS.Application.Features.Seats.Interfaces;

namespace SEVPMS.Api.Controllers.Klegar;

[ApiController]
[Route("api/events/{eventId:guid}")]
public sealed class SeatViewsController(
    ISeatService seats,
    KlegarAuthorizationService authorization) : ControllerBase
{
    [AllowAnonymous]
    [HttpGet("seats/{seatId:guid}/view")]
    public async Task<ActionResult<SeatViewAssetDto>> Get(
        Guid eventId,
        Guid seatId,
        CancellationToken ct)
    {
        var result = await seats.GetSeatViewAsync(eventId, seatId, ct);
        return result is null ? NotFound() : Ok(result);
    }

    [Authorize]
    [HttpPut("seat-view-assets")]
    public async Task<ActionResult<SeatViewAssetDto>> Upsert(
        Guid eventId,
        [FromBody] UpsertSeatViewAssetRequest request,
        CancellationToken ct)
    {
        if (!await authorization.CanManageEventAsync(HttpContext, eventId, ct)) return Forbid();
        return Ok(await seats.UpsertSeatViewAsync(eventId, request, ct));
    }
}
