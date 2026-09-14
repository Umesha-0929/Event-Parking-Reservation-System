using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SEVPMS.Api.Authorization;
using SEVPMS.Application.Features.Places.DTOs;
using SEVPMS.Application.Features.Places.Interfaces;
using SEVPMS.Application.Features.Places.Validators;

namespace SEVPMS.Api.Controllers;

[ApiController]
[Route("api/places")]
public sealed class PlacesController(IPlaceFinderService placeFinderService)
    : ControllerBase
{
    [HttpGet("venues/{venueId:guid}")]
    public async Task<ActionResult<IReadOnlyList<NearbyPlaceDto>>> Browse(
        Guid venueId,
        [FromQuery] PlaceFinderRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await placeFinderService.BrowseAsync(venueId, request, cancellationToken));
        }
        catch (PlaceFinderValidationException exception)
        {
            return BadRequest(new { error = exception.Message });
        }
    }

    [HttpGet("venues/{venueId:guid}/recommendations")]
    public async Task<ActionResult<IReadOnlyList<NearbyPlaceDto>>> Recommend(
        Guid venueId,
        [FromQuery] PlaceFinderRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await placeFinderService.RecommendAsync(venueId, request, cancellationToken));
        }
        catch (PlaceFinderValidationException exception)
        {
            return BadRequest(new { error = exception.Message });
        }
    }

    [HttpPost]
    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
    public async Task<ActionResult<NearbyPlaceDto>> Create(
        ManageNearbyPlaceRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var place = await placeFinderService.CreateAsync(request, cancellationToken);
            return CreatedAtAction(nameof(Browse), new { venueId = place.VenueId }, place);
        }
        catch (PlaceFinderValidationException exception)
        {
            return BadRequest(new { error = exception.Message });
        }
    }

    [HttpPut("{placeId:guid}")]
    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
    public async Task<ActionResult<NearbyPlaceDto>> Update(
        Guid placeId,
        ManageNearbyPlaceRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await placeFinderService.UpdateAsync(placeId, request, cancellationToken));
        }
        catch (PlaceFinderValidationException exception)
        {
            return BadRequest(new { error = exception.Message });
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpDelete("{placeId:guid}")]
    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
    public async Task<IActionResult> Delete(
        Guid placeId,
        CancellationToken cancellationToken)
    {
        try
        {
            await placeFinderService.DeleteAsync(placeId, cancellationToken);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }
}
