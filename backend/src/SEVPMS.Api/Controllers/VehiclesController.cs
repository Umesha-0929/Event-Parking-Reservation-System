using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SEVPMS.Application.Features.Vehicles.DTOs;
using SEVPMS.Application.Features.Vehicles.Interfaces;
using SEVPMS.Application.Features.Vehicles.Validators;

namespace SEVPMS.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/vehicles")]
public sealed class VehiclesController(
    ISavedVehicleService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<SavedVehicleDto>>> GetAll(
        CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var vehicles = await service.GetAllAsync(
            userId,
            cancellationToken);

        return Ok(vehicles);
    }

    [HttpGet("{vehicleId:guid}")]
    public async Task<ActionResult<SavedVehicleDto>> GetById(
        Guid vehicleId,
        CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var vehicle = await service.GetByIdAsync(
            userId,
            vehicleId,
            cancellationToken);

        if (vehicle is null)
        {
            return NotFound();
        }

        return Ok(vehicle);
    }

    [HttpPost]
    public async Task<ActionResult<SavedVehicleDto>> Create(
        CreateSavedVehicleRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        try
        {
            var vehicle = await service.CreateAsync(
                userId,
                request,
                cancellationToken);

            return CreatedAtAction(
                nameof(GetById),
                new { vehicleId = vehicle.Id },
                vehicle);
        }
        catch (SavedVehicleValidationException exception)
        {
            return BadRequest(new
            {
                errors = exception.Errors
            });
        }
    }

    [HttpPut("{vehicleId:guid}")]
    public async Task<ActionResult<SavedVehicleDto>> Update(
        Guid vehicleId,
        UpdateSavedVehicleRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        try
        {
            var vehicle = await service.UpdateAsync(
                userId,
                vehicleId,
                request,
                cancellationToken);

            if (vehicle is null)
            {
                return NotFound();
            }

            return Ok(vehicle);
        }
        catch (SavedVehicleValidationException exception)
        {
            return BadRequest(new
            {
                errors = exception.Errors
            });
        }
    }

    [HttpDelete("{vehicleId:guid}")]
    public async Task<IActionResult> Delete(
        Guid vehicleId,
        CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var deleted = await service.DeleteAsync(
            userId,
            vehicleId,
            cancellationToken);

        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }

    private bool TryGetUserId(out Guid userId)
    {
        var value = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        return Guid.TryParse(value, out userId);
    }
}