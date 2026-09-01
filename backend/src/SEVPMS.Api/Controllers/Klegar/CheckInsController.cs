using Microsoft.AspNetCore.Mvc;
using SEVPMS.Api.Klegar;
using SEVPMS.Application.Features.Tickets.DTOs;
using SEVPMS.Application.Features.Tickets.Interfaces;
namespace SEVPMS.Api.Controllers.Klegar;
[ApiController]
[Route("api/events/{eventId:guid}/check-ins")]
public sealed class CheckInsController(ITicketService tickets, RequestUserResolver users) : ControllerBase
{
    [HttpPost("scan")]
    public async Task<ActionResult<CheckInTicketResponse>> Scan(Guid eventId, [FromBody] CheckInTicketRequest request, CancellationToken ct)
    {
        if (!users.TryGetUserId(HttpContext, out var scannerId)) return Unauthorized(new { error = "scanner_required" });
        if (!users.IsOrganizerOrAdmin(HttpContext)) return Forbid();
        var result = await tickets.CheckInAsync(eventId, scannerId, request, ct); return result.Succeeded ? Ok(result) : Conflict(result);
    }
}
