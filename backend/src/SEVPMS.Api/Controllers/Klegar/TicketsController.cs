using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SEVPMS.Api.Klegar;
using SEVPMS.Application.Features.Tickets.DTOs;
using SEVPMS.Application.Features.Tickets.Interfaces;

namespace SEVPMS.Api.Controllers.Klegar;

[ApiController]
[Authorize]
public sealed class TicketsController(
    ITicketService tickets,
    RequestUserResolver users,
    KlegarAuthorizationService authorization) : ControllerBase
{
    [HttpPost("api/bookings/{bookingId:guid}/tickets/issue")]
    public async Task<ActionResult<IReadOnlyList<TicketDto>>> Issue(
        Guid bookingId,
        [FromBody] IssueTicketsRequest request,
        CancellationToken ct)
    {
        if (!users.TryGetUserId(HttpContext, out _)) return Unauthorized();
        if (!users.IsOrganizerOrAdmin(HttpContext)) return Forbid();
        if (!await authorization.CanManageBookingAsync(HttpContext, bookingId, request.EventId, ct)) return Forbid();
        return Ok(await tickets.IssueAsync(bookingId, request, ct));
    }


    [HttpGet("api/tickets/mine")]
    public async Task<ActionResult<IReadOnlyList<CustomerTicketSummaryDto>>> Mine(
        [FromQuery] int page,
        [FromQuery] int pageSize,
        CancellationToken ct)
    {
        if (!users.TryGetUserId(HttpContext, out var userId)) return Unauthorized();
        return Ok(await tickets.GetMinePageAsync(
            userId,
            page == 0 ? 1 : page,
            pageSize == 0 ? 50 : pageSize,
            ct));
    }

    [HttpGet("api/bookings/{bookingId:guid}/tickets")]
    public async Task<ActionResult<IReadOnlyList<TicketDto>>> ByBooking(
        Guid bookingId,
        CancellationToken ct)
    {
        if (!users.TryGetUserId(HttpContext, out _)) return Unauthorized();
        if (!await authorization.CanAccessBookingAsync(HttpContext, bookingId, ct)) return Forbid();
        return Ok(await tickets.GetForBookingAsync(bookingId, ct));
    }

    [HttpGet("api/tickets/{ticketNo}")]
    public async Task<ActionResult<TicketDto>> ByNumber(
        string ticketNo,
        CancellationToken ct)
    {
        if (!users.TryGetUserId(HttpContext, out _)) return Unauthorized();
        if (!await authorization.CanAccessTicketAsync(HttpContext, ticketNo, ct)) return Forbid();
        var ticket = await tickets.GetByTicketNoAsync(ticketNo, ct);
        return ticket is null ? NotFound() : Ok(ticket);
    }

    [HttpPost("api/tickets/{ticketNo}/cancel")]
    public async Task<IActionResult> Cancel(
        string ticketNo,
        CancellationToken ct)
    {
        if (!users.TryGetUserId(HttpContext, out _)) return Unauthorized();
        if (!users.IsOrganizerOrAdmin(HttpContext)) return Forbid();
        if (!await authorization.CanManageTicketAsync(HttpContext, ticketNo, ct)) return Forbid();
        return await tickets.CancelAsync(ticketNo, ct)
            ? Ok(new { cancelled = true })
            : Conflict(new { cancelled = false });
    }
}
