using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SEVPMS.Api.Authorization;
using SEVPMS.Application.Features.Bookings.DTOs;
using SEVPMS.Application.Features.Bookings.Interfaces;
using SEVPMS.Application.Interfaces.Repositories;

namespace SEVPMS.Api.Controllers;

[ApiController]
[Route("api/organizer/bookings")]
[Authorize(Policy = AuthorizationPolicies.EventOrganizerOnly)]
public sealed class OrganizerBookingsController(
    IBookingService bookingService,
    IEventRepository eventRepository) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<BookingResponse>>> GetAll(
        [FromQuery] int page,
        [FromQuery] int pageSize,
        CancellationToken cancellationToken)
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(claim, out var organizerUserId))
        {
            return Unauthorized();
        }

        var events = await eventRepository.GetByOrganizerUserIdAsync(
            organizerUserId,
            cancellationToken);

        var bookings = await bookingService.GetByEventIdsPageAsync(
            events.Select(eventEntity => eventEntity.Id).ToArray(),
            page == 0 ? 1 : page,
            pageSize == 0 ? 100 : pageSize,
            cancellationToken);

        return Ok(bookings);
    }
}
