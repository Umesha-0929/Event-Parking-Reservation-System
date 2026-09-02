using Microsoft.AspNetCore.Mvc;
using SEVPMS.Application.Features.Parking.DTOs;
using SEVPMS.Application.Features.Parking.Interfaces;

namespace SEVPMS.Api.Controllers;

[ApiController]
[Route("api/parking/navigation")]
public sealed class ParkingNavigationController(
    IParkingRouteService routeService) : ControllerBase
{
    [HttpGet("route")]
    public async Task<ActionResult<ParkingRouteDto>> GetRoute(
        [FromQuery] Guid venueId,
        [FromQuery] Guid startNodeId,
        [FromQuery] Guid endNodeId,
        [FromQuery] bool accessibleOnly = false,
        CancellationToken cancellationToken = default)
    {
        var route = await routeService.FindRouteAsync(
            venueId,
            startNodeId,
            endNodeId,
            accessibleOnly,
            cancellationToken);

        if (route is null)
        {
            return NotFound();
        }

        return Ok(route);
    }
}