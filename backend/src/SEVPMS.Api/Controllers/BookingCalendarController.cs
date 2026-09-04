using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SEVPMS.Api.Authorization;
using SEVPMS.Application.Features.Calendar.DTOs;
using SEVPMS.Application.Features.Calendar.Interfaces;

namespace SEVPMS.Api.Controllers;

[ApiController]
[Route("api/bookings")]
[Authorize(Policy = AuthorizationPolicies.CustomerOnly)]
public sealed class BookingCalendarController(
    IBookingCalendarService calendarService)
    : ControllerBase
{
    [HttpGet("{bookingId:guid}/calendar")]
    public async Task<ActionResult<BookingCalendarResponse>>
        GetCalendarInfo(
            Guid bookingId,
            CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Unauthorized();
        }

        var export =
            await calendarService.GetAsync(
                userId,
                bookingId,
                cancellationToken);

        return Ok(export.Info);
    }

    [HttpGet("{bookingId:guid}/calendar.ics")]
    public async Task<IActionResult>
        DownloadCalendar(
            Guid bookingId,
            CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Unauthorized();
        }

        var export =
            await calendarService.GetAsync(
                userId,
                bookingId,
                cancellationToken);

        var bytes =
            Encoding.UTF8.GetBytes(
                export.IcsContent);

        return File(
            bytes,
            "text/calendar; charset=utf-8",
            export.FileName);
    }

    private bool TryGetCurrentUserId(
        out Guid userId)
    {
        var value =
            User.FindFirstValue(
                ClaimTypes.NameIdentifier);

        return Guid.TryParse(
            value,
            out userId);
    }
}