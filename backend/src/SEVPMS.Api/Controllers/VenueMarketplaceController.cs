using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SEVPMS.Api.Authorization;
using SEVPMS.Application.Features.VenueMarketplace.DTOs;
using SEVPMS.Application.Features.VenueMarketplace.Interfaces;

namespace SEVPMS.Api.Controllers;

[ApiController]
public sealed class VenueMarketplaceController(IVenueMarketplaceService service) : ControllerBase
{
    [AllowAnonymous]
    [HttpGet("api/venue-facilities")]
    public async Task<ActionResult<IReadOnlyList<FacilityResponse>>> GetFacilities(
        CancellationToken cancellationToken)
        => Ok(await service.GetFacilitiesAsync(false, cancellationToken));

    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
    [HttpPost("api/venue-facilities")]
    public async Task<ActionResult<FacilityResponse>> CreateFacility(
        [FromBody] UpsertFacilityRequest request,
        CancellationToken cancellationToken)
        => StatusCode(
            StatusCodes.Status201Created,
            await service.CreateFacilityAsync(request, cancellationToken));

    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
    [HttpPut("api/venue-facilities/{id:guid}")]
    public async Task<ActionResult<FacilityResponse>> UpdateFacility(
        Guid id,
        [FromBody] UpsertFacilityRequest request,
        CancellationToken cancellationToken)
        => Ok(await service.UpdateFacilityAsync(id, request, cancellationToken));

    [AllowAnonymous]
    [HttpGet("api/venues/{venueId:guid}/marketplace")]
    public async Task<ActionResult<VenueMarketplaceResponse>> GetMarketplace(
        Guid venueId,
        CancellationToken cancellationToken)
        => Ok(await service.GetVenueAsync(venueId, cancellationToken));

    [Authorize(Policy = AuthorizationPolicies.VenueOwnerOnly)]
    [HttpPut("api/venues/{venueId:guid}/marketplace/facilities")]
    public async Task<IActionResult> SetFacilities(
        Guid venueId,
        [FromBody] SetVenueFacilitiesRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId)) return Unauthorized();
        await service.SetFacilitiesAsync(userId, venueId, request, cancellationToken);
        return NoContent();
    }

    [Authorize(Policy = AuthorizationPolicies.VenueOwnerOnly)]
    [HttpPost("api/venues/{venueId:guid}/marketplace/media")]
    public async Task<ActionResult<VenueMediaResponse>> AddMedia(
        Guid venueId,
        [FromBody] AddVenueMediaRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId)) return Unauthorized();
        return StatusCode(
            StatusCodes.Status201Created,
            await service.AddMediaAsync(userId, venueId, request, cancellationToken));
    }

    [Authorize(Policy = AuthorizationPolicies.VenueOwnerOnly)]
    [HttpPost("api/venues/{venueId:guid}/marketplace/rates")]
    public async Task<ActionResult<VenueRateResponse>> AddRate(
        Guid venueId,
        [FromBody] AddVenueRateRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId)) return Unauthorized();
        return StatusCode(
            StatusCodes.Status201Created,
            await service.AddRateAsync(userId, venueId, request, cancellationToken));
    }

    [Authorize(Policy = AuthorizationPolicies.VenueOwnerOnly)]
    [HttpPost("api/venues/{venueId:guid}/marketplace/availability")]
    public async Task<ActionResult<VenueAvailabilityResponse>> AddAvailability(
        Guid venueId,
        [FromBody] AddVenueAvailabilityRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId)) return Unauthorized();
        return StatusCode(
            StatusCodes.Status201Created,
            await service.AddAvailabilityAsync(userId, venueId, request, cancellationToken));
    }

    [Authorize(Policy = AuthorizationPolicies.VenueOwnerOnly)]
    [HttpPost("api/venues/{venueId:guid}/marketplace/layout-templates")]
    public async Task<ActionResult<VenueLayoutTemplateResponse>> AddLayoutTemplate(
        Guid venueId,
        [FromBody] AddVenueLayoutTemplateRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId)) return Unauthorized();
        return StatusCode(
            StatusCodes.Status201Created,
            await service.AddLayoutTemplateAsync(userId, venueId, request, cancellationToken));
    }

    private bool TryGetUserId(out Guid userId)
        => Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out userId);
}
