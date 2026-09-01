using Microsoft.AspNetCore.Mvc;
using SEVPMS.Api.Klegar;
using SEVPMS.Application.Features.Seats.DTOs;
using SEVPMS.Application.Features.Seats.Interfaces;

namespace SEVPMS.Api.Controllers.Klegar;

[ApiController]
[Route("api/events/{eventId:guid}/seating-layout")]
public sealed class SeatingLayoutsController(
    ISeatingLayoutService layouts,
    RequestUserResolver users) : ControllerBase
{
    [HttpGet("organizer")]
    public async Task<ActionResult<SeatingLayoutDto>> GetOrganizerLayout(
        Guid eventId,
        CancellationToken ct)
    {
        if (!users.IsOrganizerOrAdmin(HttpContext))
            return Forbid();

        if (!users.TryGetUserId(HttpContext, out _))
            return Unauthorized();

        var result = await layouts.GetOrganizerLayoutAsync(
            eventId,
            ct);

        return result is null
            ? NotFound()
            : Ok(result);
    }

    [HttpPut]
    public async Task<ActionResult<SeatingLayoutDto>> ConfigureLayout(
        Guid eventId,
        [FromBody] ConfigureSeatingLayoutRequest request,
        CancellationToken ct)
    {
        if (!users.IsOrganizerOrAdmin(HttpContext))
            return Forbid();

        if (!users.TryGetUserId(
                HttpContext,
                out var organizerUserId))
        {
            return Unauthorized();
        }

        var result = await layouts.ConfigureLayoutAsync(
            eventId,
            organizerUserId,
            request,
            ct);

        return Ok(result);
    }

    [HttpPut("sections")]
    public async Task<ActionResult<SeatSectionDto>> UpsertSection(
        Guid eventId,
        [FromBody] UpsertSeatSectionRequest request,
        CancellationToken ct)
    {
        if (!users.IsOrganizerOrAdmin(HttpContext))
            return Forbid();

        if (!users.TryGetUserId(
                HttpContext,
                out var organizerUserId))
        {
            return Unauthorized();
        }

        var result = await layouts.UpsertSectionAsync(
            eventId,
            organizerUserId,
            request,
            ct);

        return Ok(result);
    }

    [HttpPut("categories")]
    public async Task<ActionResult<SeatCategoryDto>> UpsertCategory(
        Guid eventId,
        [FromBody] UpsertSeatCategoryRequest request,
        CancellationToken ct)
    {
        if (!users.IsOrganizerOrAdmin(HttpContext))
            return Forbid();

        if (!users.TryGetUserId(
                HttpContext,
                out var organizerUserId))
        {
            return Unauthorized();
        }

        var result = await layouts.UpsertCategoryAsync(
            eventId,
            organizerUserId,
            request,
            ct);

        return Ok(result);
    }

    [HttpPost("generate-seats")]
    public async Task<ActionResult<IReadOnlyCollection<SeatAvailabilityDto>>> GenerateSeats(
        Guid eventId,
        [FromBody] GenerateSeatsRequest request,
        CancellationToken ct)
    {
        if (!users.IsOrganizerOrAdmin(HttpContext))
            return Forbid();

        if (!users.TryGetUserId(
                HttpContext,
                out var organizerUserId))
        {
            return Unauthorized();
        }

        var result = await layouts.GenerateSeatsAsync(
            eventId,
            organizerUserId,
            request,
            ct);

        return Ok(result);
    }

    [HttpPut("publish")]
    public async Task<ActionResult<SeatingLayoutDto>> PublishLayout(
        Guid eventId,
        [FromBody] PublishSeatingLayoutRequest request,
        CancellationToken ct)
    {
        if (!users.IsOrganizerOrAdmin(HttpContext))
            return Forbid();

        if (!users.TryGetUserId(
                HttpContext,
                out var organizerUserId))
        {
            return Unauthorized();
        }

        var result = await layouts.PublishLayoutAsync(
            eventId,
            organizerUserId,
            request,
            ct);

        return Ok(result);
    }

    [HttpGet("published")]
    public async Task<ActionResult<PublishedSeatingLayoutDto>> GetPublishedLayout(
        Guid eventId,
        CancellationToken ct)
    {
        var result = await layouts.GetPublishedLayoutAsync(
            eventId,
            ct);

        return result is null
            ? NotFound()
            : Ok(result);
    }
}
