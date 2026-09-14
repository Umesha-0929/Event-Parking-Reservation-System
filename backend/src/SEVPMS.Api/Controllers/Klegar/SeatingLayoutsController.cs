using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SEVPMS.Api.Klegar;
using SEVPMS.Application.Features.Seats.DTOs;
using SEVPMS.Application.Features.Seats.Interfaces;

namespace SEVPMS.Api.Controllers.Klegar;

[ApiController]
[Route("api/events/{eventId:guid}/seating-layout")]
public sealed class SeatingLayoutsController(
    ISeatingLayoutService layouts,
    RequestUserResolver users,
    KlegarAuthorizationService authorization) : ControllerBase
{
    [Authorize]
    [HttpGet("organizer")]
    public async Task<ActionResult<SeatingLayoutDto>> GetOrganizerLayout(
        Guid eventId,
        CancellationToken ct)
    {
        if (!users.IsOrganizerOrAdmin(HttpContext)) return Forbid();
        if (!users.TryGetUserId(HttpContext, out _)) return Unauthorized();
        if (!await authorization.CanManageEventAsync(HttpContext, eventId, ct)) return Forbid();

        var result = await layouts.GetOrganizerLayoutAsync(eventId, ct);
        return result is null ? NotFound() : Ok(result);
    }

    [Authorize]
    [HttpPut]
    public async Task<ActionResult<SeatingLayoutDto>> ConfigureLayout(
        Guid eventId,
        [FromBody] ConfigureSeatingLayoutRequest request,
        CancellationToken ct)
    {
        if (!users.IsOrganizerOrAdmin(HttpContext)) return Forbid();
        if (!users.TryGetUserId(HttpContext, out var organizerUserId)) return Unauthorized();
        if (!await authorization.CanManageEventAsync(HttpContext, eventId, ct)) return Forbid();
        return Ok(await layouts.ConfigureLayoutAsync(eventId, organizerUserId, request, ct));
    }

    [Authorize]
    [HttpPut("sections")]
    public async Task<ActionResult<SeatSectionDto>> UpsertSection(
        Guid eventId,
        [FromBody] UpsertSeatSectionRequest request,
        CancellationToken ct)
    {
        if (!users.IsOrganizerOrAdmin(HttpContext)) return Forbid();
        if (!users.TryGetUserId(HttpContext, out var organizerUserId)) return Unauthorized();
        if (!await authorization.CanManageEventAsync(HttpContext, eventId, ct)) return Forbid();
        return Ok(await layouts.UpsertSectionAsync(eventId, organizerUserId, request, ct));
    }

    [Authorize]
    [HttpPut("categories")]
    public async Task<ActionResult<SeatCategoryDto>> UpsertCategory(
        Guid eventId,
        [FromBody] UpsertSeatCategoryRequest request,
        CancellationToken ct)
    {
        if (!users.IsOrganizerOrAdmin(HttpContext)) return Forbid();
        if (!users.TryGetUserId(HttpContext, out var organizerUserId)) return Unauthorized();
        if (!await authorization.CanManageEventAsync(HttpContext, eventId, ct)) return Forbid();
        return Ok(await layouts.UpsertCategoryAsync(eventId, organizerUserId, request, ct));
    }

    [Authorize]
    [HttpPost("generate-seats")]
    public async Task<ActionResult<IReadOnlyCollection<SeatAvailabilityDto>>> GenerateSeats(
        Guid eventId,
        [FromBody] GenerateSeatsRequest request,
        CancellationToken ct)
    {
        if (!users.IsOrganizerOrAdmin(HttpContext)) return Forbid();
        if (!users.TryGetUserId(HttpContext, out var organizerUserId)) return Unauthorized();
        if (!await authorization.CanManageEventAsync(HttpContext, eventId, ct)) return Forbid();
        return Ok(await layouts.GenerateSeatsAsync(eventId, organizerUserId, request, ct));
    }

    [Authorize]
    [HttpPut("publish")]
    public async Task<ActionResult<SeatingLayoutDto>> PublishLayout(
        Guid eventId,
        [FromBody] PublishSeatingLayoutRequest request,
        CancellationToken ct)
    {
        if (!users.IsOrganizerOrAdmin(HttpContext)) return Forbid();
        if (!users.TryGetUserId(HttpContext, out var organizerUserId)) return Unauthorized();
        if (!await authorization.CanManageEventAsync(HttpContext, eventId, ct)) return Forbid();
        return Ok(await layouts.PublishLayoutAsync(eventId, organizerUserId, request, ct));
    }

    [AllowAnonymous]
    [HttpGet("published")]
    public async Task<ActionResult<PublishedSeatingLayoutDto>> GetPublishedLayout(
        Guid eventId,
        CancellationToken ct)
    {
        var result = await layouts.GetPublishedLayoutAsync(eventId, ct);
        return result is null ? NotFound() : Ok(result);
    }
}
