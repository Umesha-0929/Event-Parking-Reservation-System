using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SEVPMS.Api.Authorization;
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

    [HttpPost("zones")]
    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
    public async Task<ActionResult<ParkingZoneDto>> CreateZone(
        UpsertParkingZoneRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var zone = await service.CreateZoneAsync(
                request,
                cancellationToken);

            return CreatedAtAction(
                nameof(GetZonesByVenue),
                new
                {
                    venueId = zone.VenueId
                },
                zone);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(
                new
                {
                    error = exception.Message
                });
        }
    }

    [HttpPut("zones/{parkingZoneId:guid}")]
    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
    public async Task<ActionResult<ParkingZoneDto>> UpdateZone(
        Guid parkingZoneId,
        UpsertParkingZoneRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var zone = await service.UpdateZoneAsync(
                parkingZoneId,
                request,
                cancellationToken);

            return Ok(zone);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(
                new
                {
                    error = exception.Message
                });
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpDelete("zones/{parkingZoneId:guid}")]
    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
    public async Task<IActionResult> DeleteZone(
        Guid parkingZoneId,
        CancellationToken cancellationToken)
    {
        try
        {
            var deleted = await service.DeleteZoneAsync(
                parkingZoneId,
                cancellationToken);

            if (!deleted)
            {
                return NotFound();
            }

            return NoContent();
        }
        catch (ArgumentException exception)
        {
            return BadRequest(
                new
                {
                    error = exception.Message
                });
        }
    }

    [HttpPost("slots")]
    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
    public async Task<ActionResult<ParkingSlotDto>> CreateSlot(
        UpsertParkingSlotRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var slot = await service.CreateSlotAsync(
                request,
                cancellationToken);

            return CreatedAtAction(
                nameof(GetSlotById),
                new
                {
                    parkingSlotId = slot.Id
                },
                slot);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(
                new
                {
                    error = exception.Message
                });
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(
                new
                {
                    error = exception.Message
                });
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPut("slots/{parkingSlotId:guid}")]
    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
    public async Task<ActionResult<ParkingSlotDto>> UpdateSlot(
        Guid parkingSlotId,
        UpsertParkingSlotRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var slot = await service.UpdateSlotAsync(
                parkingSlotId,
                request,
                cancellationToken);

            return Ok(slot);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(
                new
                {
                    error = exception.Message
                });
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(
                new
                {
                    error = exception.Message
                });
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpDelete("slots/{parkingSlotId:guid}")]
    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
    public async Task<IActionResult> DeleteSlot(
        Guid parkingSlotId,
        CancellationToken cancellationToken)
    {
        try
        {
            var deleted = await service.DeleteSlotAsync(
                parkingSlotId,
                cancellationToken);

            if (!deleted)
            {
                return NotFound();
            }

            return NoContent();
        }
        catch (ArgumentException exception)
        {
            return BadRequest(
                new
                {
                    error = exception.Message
                });
        }
    }
}