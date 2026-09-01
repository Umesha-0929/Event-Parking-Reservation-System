using Microsoft.AspNetCore.Mvc;
using SEVPMS.Api.Klegar;
using SEVPMS.Application.Features.Tickets.DTOs;
using SEVPMS.Application.Features.Tickets.Interfaces;
namespace SEVPMS.Api.Controllers.Klegar;
[ApiController]
public sealed class TicketsController(ITicketService tickets, RequestUserResolver users) : ControllerBase
{
    [HttpPost("api/bookings/{bookingId:guid}/tickets/issue")]
    public async Task<ActionResult<IReadOnlyList<TicketDto>>> Issue(Guid bookingId, [FromBody] IssueTicketsRequest request, CancellationToken ct)
    {
        if (!users.IsOrganizerOrAdmin(HttpContext)) return Forbid();
        return Ok(await tickets.IssueAsync(bookingId, request, ct));
    }
    [HttpGet("api/bookings/{bookingId:guid}/tickets")]
    public async Task<ActionResult<IReadOnlyList<TicketDto>>> ByBooking(Guid bookingId, CancellationToken ct) => Ok(await tickets.GetForBookingAsync(bookingId, ct));
    [HttpGet("api/tickets/{ticketNo}")]
    public async Task<ActionResult<TicketDto>> ByNumber(string ticketNo, CancellationToken ct) { var t = await tickets.GetByTicketNoAsync(ticketNo, ct); return t is null ? NotFound() : Ok(t); }
    [HttpPost("api/tickets/{ticketNo}/cancel")]
    public async Task<IActionResult> Cancel(string ticketNo, CancellationToken ct)
    {
        if (!users.IsOrganizerOrAdmin(HttpContext)) return Forbid();
        return await tickets.CancelAsync(ticketNo, ct) ? Ok(new { cancelled = true }) : Conflict(new { cancelled = false });
    }
}
