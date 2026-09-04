using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SEVPMS.Api.Authorization;
using SEVPMS.Application.Features.Waitlists.DTOs;
using SEVPMS.Application.Features.Waitlists.Interfaces;

namespace SEVPMS.Api.Controllers;

[ApiController]
[Route("api/waitlists")]
[Authorize(Policy = AuthorizationPolicies.CustomerOnly)]
public sealed class WaitlistsController(
    IWaitlistService waitlistService)
    : ControllerBase
{
    [HttpGet("events/{eventId:guid}/me")]
    public async Task<ActionResult<WaitlistEntryDto>> GetMine(
        Guid eventId,
        CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId))
            return Unauthorized();

        try
        {
            var entry =
                await waitlistService.GetMineAsync(
                    userId,
                    eventId,
                    cancellationToken);

            return entry is null
                ? NotFound()
                : Ok(entry);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new
            {
                error = exception.Message
            });
        }
    }

    [HttpPost("events/{eventId:guid}")]
    public async Task<ActionResult<WaitlistEntryDto>> Join(
        Guid eventId,
        CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId))
            return Unauthorized();

        try
        {
            var entry =
                await waitlistService.JoinAsync(
                    userId,
                    eventId,
                    cancellationToken);

            return CreatedAtAction(
                nameof(GetMine),
                new
                {
                    eventId
                },
                entry);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new
            {
                error = exception.Message
            });
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(new
            {
                error = exception.Message
            });
        }
    }

    [HttpDelete("events/{eventId:guid}")]
    public async Task<IActionResult> Leave(
        Guid eventId,
        CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId))
            return Unauthorized();

        try
        {
            var removed =
                await waitlistService.LeaveAsync(
                    userId,
                    eventId,
                    cancellationToken);

            return removed
                ? NoContent()
                : NotFound();
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new
            {
                error = exception.Message
            });
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(new
            {
                error = exception.Message
            });
        }
    }

    private bool TryGetUserId(
        out Guid userId)
    {
        var value =
            User.FindFirstValue(
                ClaimTypes.NameIdentifier);

        return Guid.TryParse(
            value,
            out userId);
    }
}