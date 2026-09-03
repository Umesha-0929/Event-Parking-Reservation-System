using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SEVPMS.Api.Authorization;
using SEVPMS.Application.Common.Exceptions;
using SEVPMS.Application.Features.Parking.DTOs;
using SEVPMS.Application.Features.Parking.Interfaces;
using SEVPMS.Application.Features.Parking.Validators;

namespace SEVPMS.Api.Controllers;

[ApiController]
[Route("api/parking/reservations")]
[Authorize(Policy = AuthorizationPolicies.CustomerOnly)]
public sealed class ParkingReservationsController(
    IParkingReservationService reservationService)
    : ControllerBase
{
    [HttpGet("{reservationId:guid}")]
    public async Task<ActionResult<ParkingReservationDto>> GetById(
        Guid reservationId,
        CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        try
        {
            var reservation = await reservationService.GetByIdAsync(
                userId,
                reservationId,
                cancellationToken);

            return reservation is null ? NotFound() : Ok(reservation);
        }
        catch (ForbiddenAccessException)
        {
            return Forbid();
        }
    }

    [HttpPost]
    public async Task<ActionResult<ParkingReservationDto>> Create(
        CreateParkingReservationRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        try
        {
            var reservation = await reservationService.CreateAsync(
                userId,
                request,
                cancellationToken);

            return CreatedAtAction(
                nameof(GetById),
                new { reservationId = reservation.Id },
                reservation);
        }
        catch (ParkingReservationValidationException exception)
        {
            return BadRequest(new { error = exception.Message });
        }
        catch (ForbiddenAccessException)
        {
            return Forbid();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPost("{reservationId:guid}/enter")]
    public Task<ActionResult<ParkingReservationDto>> Enter(
        Guid reservationId,
        CancellationToken cancellationToken)
        => RunTransitionAsync(
            reservationId,
            (userId, id, ct) => reservationService.MarkEnteredAsync(userId, id, ct),
            cancellationToken);

    [HttpPost("{reservationId:guid}/park")]
    public Task<ActionResult<ParkingReservationDto>> Park(
        Guid reservationId,
        CancellationToken cancellationToken)
        => RunTransitionAsync(
            reservationId,
            (userId, id, ct) => reservationService.MarkParkedAsync(userId, id, ct),
            cancellationToken);

    [HttpPost("{reservationId:guid}/exit")]
    public Task<ActionResult<ParkingReservationDto>> Exit(
        Guid reservationId,
        CancellationToken cancellationToken)
        => RunTransitionAsync(
            reservationId,
            (userId, id, ct) => reservationService.MarkExitedAsync(userId, id, ct),
            cancellationToken);

    [HttpPost("scan")]
    public async Task<ActionResult<ParkingReservationDto>> Scan(
        ParkingQrScanRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        try
        {
            return Ok(await reservationService.ScanAsync(userId, request, cancellationToken));
        }
        catch (ParkingReservationValidationException exception)
        {
            return BadRequest(new { error = exception.Message });
        }
        catch (ForbiddenAccessException)
        {
            return Forbid();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpDelete("{reservationId:guid}")]
    public async Task<IActionResult> Cancel(
        Guid reservationId,
        CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        try
        {
            await reservationService.CancelAsync(userId, reservationId, cancellationToken);
            return NoContent();
        }
        catch (ParkingReservationValidationException exception)
        {
            return BadRequest(new { error = exception.Message });
        }
        catch (ForbiddenAccessException)
        {
            return Forbid();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    private async Task<ActionResult<ParkingReservationDto>> RunTransitionAsync(
        Guid reservationId,
        Func<Guid, Guid, CancellationToken, Task<ParkingReservationDto>> transition,
        CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        try
        {
            return Ok(await transition(userId, reservationId, cancellationToken));
        }
        catch (ParkingReservationValidationException exception)
        {
            return BadRequest(new { error = exception.Message });
        }
        catch (ForbiddenAccessException)
        {
            return Forbid();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    private bool TryGetUserId(out Guid userId)
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(value, out userId);
    }
}
