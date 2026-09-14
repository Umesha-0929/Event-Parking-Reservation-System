using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SEVPMS.Api.Authorization;
using SEVPMS.Application.Features.Reviews.DTOs;
using SEVPMS.Application.Features.Reviews.Interfaces;

namespace SEVPMS.Api.Controllers;

[ApiController]
[Route("api/events/{eventId:guid}/reviews")]
public sealed class ReviewsController(
    IEventReviewService reviewService)
    : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IReadOnlyList<EventReviewDto>>>
        GetByEvent(
            Guid eventId,
            CancellationToken cancellationToken)
    {
        try
        {
            var reviews =
                await reviewService.GetByEventAsync(
                    eventId,
                    cancellationToken);

            return Ok(reviews);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new
            {
                error = exception.Message
            });
        }
    }

    [HttpGet("summary")]
    [AllowAnonymous]
    public async Task<ActionResult<EventRatingSummaryDto>>
        GetSummary(
            Guid eventId,
            CancellationToken cancellationToken)
    {
        try
        {
            var summary =
                await reviewService.GetSummaryAsync(
                    eventId,
                    cancellationToken);

            return Ok(summary);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new
            {
                error = exception.Message
            });
        }
    }

    [HttpPost]
    [Authorize(Policy = AuthorizationPolicies.CustomerOnly)]
    public async Task<ActionResult<EventReviewDto>>
        Create(
            Guid eventId,
            CreateEventReviewRequest request,
            CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        try
        {
            var review =
                await reviewService.CreateAsync(
                    userId,
                    eventId,
                    request,
                    cancellationToken);

            return Created(
                $"/api/events/{eventId}/reviews",
                review);
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
