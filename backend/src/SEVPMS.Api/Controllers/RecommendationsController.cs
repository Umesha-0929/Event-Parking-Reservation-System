using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SEVPMS.Api.Authorization;
using SEVPMS.Application.Features.Recommendations.DTOs;
using SEVPMS.Application.Features.Recommendations.Interfaces;

namespace SEVPMS.Api.Controllers;

[ApiController]
[Route("api/recommendations")]
[Authorize(Policy = AuthorizationPolicies.CustomerOnly)]
public sealed class RecommendationsController(
    IEventRecommendationService recommendationService)
    : ControllerBase
{
    [HttpGet("events")]
    public async Task<ActionResult<
        IReadOnlyList<EventRecommendationDto>>>
        GetEventRecommendations(
            [FromQuery]
            EventRecommendationRequest request,
            CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        try
        {
            var recommendations =
                await recommendationService
                    .GetRecommendationsAsync(
                        userId,
                        request,
                        cancellationToken);

            return Ok(recommendations);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new
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