using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SEVPMS.Api.Authorization;
using SEVPMS.Application.Features.VenueRentals.DTOs;
using SEVPMS.Application.Features.VenueRentals.Interfaces;

namespace SEVPMS.Api.Controllers;

[ApiController]
[Route("api/venue-rentals")]
public sealed class VenueRentalsController(IVenueRentalService rentalService) : ControllerBase
{
    [Authorize(Policy = AuthorizationPolicies.EventOrganizerOnly)]
    [HttpGet("mine")]
    public async Task<ActionResult<IReadOnlyList<VenueRentalResponse>>> GetMine(CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var userId)) return Unauthorized();
        return Ok(await rentalService.GetMineAsync(userId, cancellationToken));
    }

    [Authorize(Policy = AuthorizationPolicies.VenueOwnerOnly)]
    [HttpGet("incoming")]
    public async Task<ActionResult<IReadOnlyList<VenueRentalResponse>>> GetIncoming(CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var userId)) return Unauthorized();
        return Ok(await rentalService.GetIncomingAsync(userId, cancellationToken));
    }

    [Authorize(Policy = AuthorizationPolicies.EventOrganizerOnly)]
    [HttpPost]
    public async Task<ActionResult<VenueRentalResponse>> Create([FromBody] CreateVenueRentalRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var userId)) return Unauthorized();
        var response = await rentalService.CreateAsync(userId, request, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, response);
    }

    [Authorize(Policy = AuthorizationPolicies.VenueOwnerOnly)]
    [HttpPut("{id:guid}/status")]
    public async Task<ActionResult<VenueRentalResponse>> UpdateStatus(Guid id, [FromBody] UpdateVenueRentalStatusRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var userId)) return Unauthorized();
        return Ok(await rentalService.UpdateStatusAsync(userId, id, request, cancellationToken));
    }

    [Authorize(Policy = AuthorizationPolicies.EventOrganizerOnly)]
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<VenueRentalResponse>> Cancel(Guid id, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var userId)) return Unauthorized();
        return Ok(await rentalService.CancelAsync(userId, id, cancellationToken));
    }


    private bool TryGetCurrentUserId(out Guid userId)
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(value, out userId);
    }

}
