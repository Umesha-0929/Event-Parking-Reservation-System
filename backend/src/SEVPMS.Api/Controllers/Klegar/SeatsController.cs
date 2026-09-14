using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SEVPMS.Api.Klegar;
using SEVPMS.Application.Features.Seats.DTOs;
using SEVPMS.Application.Features.Seats.Interfaces;

namespace SEVPMS.Api.Controllers.Klegar;

[ApiController]
[Route("api/events/{eventId:guid}/seats")]
public sealed class SeatsController(
    ISeatService seats,
    KlegarAuthorizationService authorization) : ControllerBase
{
    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<SeatAvailabilityDto>>> Get(
        Guid eventId,
        [FromQuery] Guid? sectionId,
        CancellationToken ct)
        => Ok(await seats.GetAvailabilityAsync(eventId, sectionId, ct));

    [Authorize]
    [HttpPut]
    public async Task<ActionResult<SeatAvailabilityDto>> Upsert(
        Guid eventId,
        [FromBody] UpsertSeatRequest request,
        CancellationToken ct)
    {
        if (!await authorization.CanManageEventAsync(HttpContext, eventId, ct)) return Forbid();
        return Ok(await seats.UpsertSeatAsync(eventId, request, ct));
    }
}
