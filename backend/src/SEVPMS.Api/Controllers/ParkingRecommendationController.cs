using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SEVPMS.Application.Features.Parking.DTOs;
using SEVPMS.Application.Features.Parking.Interfaces;

namespace SEVPMS.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/parking/recommendations")]
public sealed class ParkingRecommendationController(
    IParkingRecommendationCandidateProvider candidateProvider,
    IParkingRecommendationService recommendationService)
    : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<ParkingRecommendationDto>> Recommend(
        ParkingRecommendationRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var candidates = await candidateProvider.GetCandidatesAsync(
            request,
            userId,
            cancellationToken);

        var recommendation =
            recommendationService.RecommendBestSlot(
                candidates,
                request.RequiresAccessibleParking);

        if (recommendation is null)
        {
            return NotFound();
        }

        return Ok(recommendation);
    }

    private bool TryGetUserId(out Guid userId)
    {
        var value = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        return Guid.TryParse(value, out userId);
    }
}