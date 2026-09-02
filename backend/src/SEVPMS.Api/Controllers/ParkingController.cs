using Microsoft.AspNetCore.Mvc;
using SEVPMS.Application.Features.Parking.DTOs;
using SEVPMS.Application.Features.Parking.Interfaces;

namespace SEVPMS.Api.Controllers;

[ApiController]
[Route("api/parking")]
public sealed class ParkingController(
    IParkingService service) : ControllerBase
{
    [HttpGet("venues/{venueId:guid}/zones")]
    public async Task<ActionResult<IReadOnlyList<ParkingZoneDto>>> GetZonesByVenue(
        Guid venueId,
        CancellationToken cancellationToken)
    {
        var zones = await service.GetZonesByVenueAsync(
            venueId,
            cancellationToken);

        return Ok(zones);
    }

    [HttpGet("zones/{parkingZoneId:guid}/slots")]
    public async Task<ActionResult<IReadOnlyList<ParkingSlotDto>>> GetSlotsByZone(
        Guid parkingZoneId,
        CancellationToken cancellationToken)
    {
        var slots = await service.GetSlotsByZoneAsync(
            parkingZoneId,
            cancellationToken);

        return Ok(slots);
    }

    [HttpGet("slots/{parkingSlotId:guid}")]
    public async Task<ActionResult<ParkingSlotDto>> GetSlotById(
        Guid parkingSlotId,
        CancellationToken cancellationToken)
    {
        var slot = await service.GetSlotByIdAsync(
            parkingSlotId,
            cancellationToken);

        if (slot is null)
        {
            return NotFound();
        }

        return Ok(slot);
    }
}