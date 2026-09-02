using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SEVPMS.Api.Authorization;
using SEVPMS.Application.Features.Venues.DTOs;
using SEVPMS.Application.Features.Venues.Interfaces;

namespace SEVPMS.Api.Controllers;

[ApiController]
[Route("api/venues")]
public sealed class VenuesController(
    IVenueService venueService)
    : ControllerBase
{
    [AllowAnonymous]
    [HttpGet]
    public async Task<
        ActionResult<IReadOnlyList<VenueResponse>>>
        GetActiveVenues(
            CancellationToken cancellationToken)
    {
        var venues =
            await venueService.GetActiveVenuesAsync(
                cancellationToken);

        return Ok(venues);
    }

    [AllowAnonymous]
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<VenueResponse>>
        GetById(
            Guid id,
            CancellationToken cancellationToken)
    {
        var venue =
            await venueService.GetByIdAsync(
                id,
                cancellationToken);

        return Ok(venue);
    }

    [Authorize(
        Policy = AuthorizationPolicies.VenueOwnerOnly)]
    [HttpGet("mine")]
    public async Task<
        ActionResult<IReadOnlyList<VenueResponse>>>
        GetMyVenues(
            CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Unauthorized();
        }

        var venues =
            await venueService.GetMyVenuesAsync(
                userId,
                cancellationToken);

        return Ok(venues);
    }

    [Authorize(
        Policy = AuthorizationPolicies.VenueOwnerOnly)]
    [HttpPost]
    public async Task<ActionResult<VenueResponse>>
        Create(
            [FromBody] CreateVenueRequest request,
            CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Unauthorized();
        }

        var venue =
            await venueService.CreateAsync(
                userId,
                request,
                cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new { id = venue.VenueId },
            venue);
    }

    [Authorize(
        Policy = AuthorizationPolicies.VenueOwnerOnly)]
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<VenueResponse>>
        Update(
            Guid id,
            [FromBody] UpdateVenueRequest request,
            CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Unauthorized();
        }

        var venue =
            await venueService.UpdateAsync(
                userId,
                id,
                request,
                cancellationToken);

        return Ok(venue);
    }

    [Authorize(
        Policy = AuthorizationPolicies.VenueOwnerOnly)]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Deactivate(
        Guid id,
        CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Unauthorized();
        }

        await venueService.DeactivateAsync(
            userId,
            id,
            cancellationToken);

        return NoContent();
    }

    private bool TryGetCurrentUserId(
        out Guid userId)
    {
        var userIdValue =
            User.FindFirstValue(
                ClaimTypes.NameIdentifier);

        return Guid.TryParse(
            userIdValue,
            out userId);
    }
}