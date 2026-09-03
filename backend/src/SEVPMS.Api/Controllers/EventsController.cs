using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SEVPMS.Api.Authorization;
using SEVPMS.Application.Features.Events.DTOs;
using SEVPMS.Application.Features.Events.Interfaces;

namespace SEVPMS.Api.Controllers;

[ApiController]
[Route("api/events")]
public sealed class EventsController(IEventService eventService) : ControllerBase
{
    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<EventResponse>>>
        GetPublished(
            [FromQuery] EventSearchRequest request,
            CancellationToken cancellationToken)
        => Ok(await eventService
            .GetPublishedAsync(
                request,
                cancellationToken));

    [AllowAnonymous]
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<EventResponse>> GetById(Guid id, CancellationToken cancellationToken)
        => Ok(await eventService.GetPublicByIdAsync(id, cancellationToken));

    [Authorize(Policy = AuthorizationPolicies.EventOrganizerOnly)]
    [HttpGet("mine")]
    public async Task<ActionResult<IReadOnlyList<EventResponse>>> GetMine(CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var userId)) return Unauthorized();
        return Ok(await eventService.GetMineAsync(userId, cancellationToken));
    }

    [Authorize(Policy = AuthorizationPolicies.EventOrganizerOnly)]
    [HttpPost]
    public async Task<ActionResult<EventResponse>> Create([FromBody] CreateEventRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var userId)) return Unauthorized();
        var response = await eventService.CreateAsync(userId, request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = response.EventId }, response);
    }

    [Authorize(Policy = AuthorizationPolicies.EventOrganizerOnly)]
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<EventResponse>> Update(Guid id, [FromBody] UpdateEventRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var userId)) return Unauthorized();
        return Ok(await eventService.UpdateAsync(userId, id, request, cancellationToken));
    }

    [Authorize(Policy = AuthorizationPolicies.EventOrganizerOnly)]
    [HttpPut("{id:guid}/publish")]
    public async Task<ActionResult<EventResponse>> Publish(Guid id, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var userId)) return Unauthorized();
        return Ok(await eventService.PublishAsync(userId, id, cancellationToken));
    }

    [Authorize(Policy = AuthorizationPolicies.EventOrganizerOnly)]
    [HttpPut("{id:guid}/cancel")]
    public async Task<ActionResult<EventResponse>> Cancel(Guid id, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var userId)) return Unauthorized();
        return Ok(await eventService.CancelAsync(userId, id, cancellationToken));
    }


    private bool TryGetCurrentUserId(out Guid userId)
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(value, out userId);
    }

}
