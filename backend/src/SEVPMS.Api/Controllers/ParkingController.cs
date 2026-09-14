using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SEVPMS.Api.Authorization;
using SEVPMS.Application.Features.Parking.DTOs;
using SEVPMS.Application.Features.Parking.Interfaces;
using SEVPMS.Application.Interfaces.Repositories;

namespace SEVPMS.Api.Controllers;

[ApiController]
[Route("api/parking")]
public sealed class ParkingController(
    IParkingService service,
    IParkingRepository parkingRepository,
    IParkingRouteRepository parkingRouteRepository,
    IVenueRepository venueRepository) : ControllerBase
{
    [HttpGet("venues/{venueId:guid}/nodes")]
    public async Task<ActionResult<IReadOnlyList<ParkingNodeDto>>> GetNodesByVenue(
        Guid venueId,
        CancellationToken cancellationToken)
    {
        var nodes = await parkingRouteRepository.GetNodesByVenueAsync(venueId, cancellationToken);
        return Ok(nodes.OrderBy(x => x.NodeCode).Select(x => new ParkingNodeDto
        {
            Id = x.Id, VenueId = x.VenueId, LayoutId = x.LayoutId, NodeCode = x.NodeCode,
            X = x.X, Y = x.Y, NodeType = x.NodeType
        }).ToList());
    }

    [HttpGet("venues/{venueId:guid}/zones")]
    public async Task<ActionResult<IReadOnlyList<ParkingZoneDto>>> GetZonesByVenue(
        Guid venueId,
        [FromQuery] int page,
        [FromQuery] int pageSize,
        CancellationToken cancellationToken)
    {
        var zones = await service.GetZonesByVenuePageAsync(
            venueId,
            page == 0 ? 1 : page,
            pageSize == 0 ? 50 : pageSize,
            cancellationToken);

        return Ok(zones);
    }

    [HttpGet("zones/{parkingZoneId:guid}/slots")]
    public async Task<ActionResult<IReadOnlyList<ParkingSlotDto>>> GetSlotsByZone(
        Guid parkingZoneId,
        [FromQuery] int page,
        [FromQuery] int pageSize,
        CancellationToken cancellationToken)
    {
        var slots = await service.GetSlotsByZonePageAsync(
            parkingZoneId,
            page == 0 ? 1 : page,
            pageSize == 0 ? 100 : pageSize,
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
    [Authorize(Policy = AuthorizationPolicies.ParkingManager)]
    public async Task<ActionResult<ParkingZoneDto>> CreateZone(
        UpsertParkingZoneRequest request,
        CancellationToken cancellationToken)
    {
        if (!await CanManageVenueAsync(request.VenueId, cancellationToken))
        {
            return Forbid();
        }

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
    [Authorize(Policy = AuthorizationPolicies.ParkingManager)]
    public async Task<ActionResult<ParkingZoneDto>> UpdateZone(
        Guid parkingZoneId,
        UpsertParkingZoneRequest request,
        CancellationToken cancellationToken)
    {
        var existingZone = await parkingRepository.GetZoneByIdAsync(
            parkingZoneId,
            cancellationToken);

        if (existingZone is null)
        {
            return NotFound();
        }

        if (!await CanManageVenueAsync(existingZone.VenueId, cancellationToken) ||
            !await CanManageVenueAsync(request.VenueId, cancellationToken))
        {
            return Forbid();
        }

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
    [Authorize(Policy = AuthorizationPolicies.ParkingManager)]
    public async Task<IActionResult> DeleteZone(
        Guid parkingZoneId,
        CancellationToken cancellationToken)
    {
        var existingZone = await parkingRepository.GetZoneByIdAsync(
            parkingZoneId,
            cancellationToken);

        if (existingZone is null)
        {
            return NotFound();
        }

        if (!await CanManageVenueAsync(existingZone.VenueId, cancellationToken))
        {
            return Forbid();
        }

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

    [HttpPost("slots/bulk")]
    [Authorize(Policy = AuthorizationPolicies.ParkingManager)]
    public async Task<ActionResult<IReadOnlyList<ParkingSlotDto>>> CreateSlotsBulk(
        BulkCreateParkingSlotsRequest request,
        CancellationToken cancellationToken)
    {
        if (request.Slots.Count == 0) return Ok(Array.Empty<ParkingSlotDto>());
        var zoneId = request.Slots[0].ParkingZoneId;
        if (request.Slots.Any(x => x.ParkingZoneId != zoneId))
            return BadRequest(new { error = "All bulk parking slots must belong to the same zone." });

        var zone = await parkingRepository.GetZoneByIdAsync(zoneId, cancellationToken);
        if (zone is null) return NotFound();
        if (!await CanManageVenueAsync(zone.VenueId, cancellationToken)) return Forbid();

        try
        {
            return Ok(await service.CreateSlotsBulkAsync(request.Slots, cancellationToken));
        }
        catch (ArgumentException exception) { return BadRequest(new { error = exception.Message }); }
        catch (InvalidOperationException exception) { return Conflict(new { error = exception.Message }); }
        catch (KeyNotFoundException) { return NotFound(); }
    }

    [HttpPost("slots")]
    [Authorize(Policy = AuthorizationPolicies.ParkingManager)]
    public async Task<ActionResult<ParkingSlotDto>> CreateSlot(
        UpsertParkingSlotRequest request,
        CancellationToken cancellationToken)
    {
        var zone = await parkingRepository.GetZoneByIdAsync(
            request.ParkingZoneId,
            cancellationToken);

        if (zone is null)
        {
            return NotFound();
        }

        if (!await CanManageVenueAsync(zone.VenueId, cancellationToken))
        {
            return Forbid();
        }

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
    [Authorize(Policy = AuthorizationPolicies.ParkingManager)]
    public async Task<ActionResult<ParkingSlotDto>> UpdateSlot(
        Guid parkingSlotId,
        UpsertParkingSlotRequest request,
        CancellationToken cancellationToken)
    {
        var existingSlot = await parkingRepository.GetSlotByIdAsync(
            parkingSlotId,
            cancellationToken);
        if (existingSlot is null)
        {
            return NotFound();
        }

        var existingZone = await parkingRepository.GetZoneByIdAsync(
            existingSlot.ParkingZoneId,
            cancellationToken);
        var requestedZone = await parkingRepository.GetZoneByIdAsync(
            request.ParkingZoneId,
            cancellationToken);

        if (existingZone is null || requestedZone is null)
        {
            return NotFound();
        }

        if (!await CanManageVenueAsync(existingZone.VenueId, cancellationToken) ||
            !await CanManageVenueAsync(requestedZone.VenueId, cancellationToken))
        {
            return Forbid();
        }

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
    [Authorize(Policy = AuthorizationPolicies.ParkingManager)]
    public async Task<IActionResult> DeleteSlot(
        Guid parkingSlotId,
        CancellationToken cancellationToken)
    {
        var existingSlot = await parkingRepository.GetSlotByIdAsync(
            parkingSlotId,
            cancellationToken);
        if (existingSlot is null)
        {
            return NotFound();
        }

        var zone = await parkingRepository.GetZoneByIdAsync(
            existingSlot.ParkingZoneId,
            cancellationToken);
        if (zone is null)
        {
            return NotFound();
        }

        if (!await CanManageVenueAsync(zone.VenueId, cancellationToken))
        {
            return Forbid();
        }

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

    private async Task<bool> CanManageVenueAsync(
        Guid venueId,
        CancellationToken cancellationToken)
    {
        if (User.IsInRole("Admin"))
        {
            return true;
        }

        if (!User.IsInRole("VenueOwner") ||
            !Guid.TryParse(
                User.FindFirstValue(ClaimTypes.NameIdentifier),
                out var userId))
        {
            return false;
        }

        var venue = await venueRepository.GetByIdAsync(
            venueId,
            cancellationToken);

        return venue is not null &&
               venue.OwnerUserId == userId;
    }

}