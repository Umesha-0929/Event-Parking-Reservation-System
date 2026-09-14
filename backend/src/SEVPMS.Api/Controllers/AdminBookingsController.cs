using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SEVPMS.Api.Authorization;
using SEVPMS.Application.Features.Bookings.DTOs;
using SEVPMS.Application.Features.Bookings.Interfaces;

namespace SEVPMS.Api.Controllers;

[ApiController]
[Route("api/admin/bookings")]
[Authorize(Policy = AuthorizationPolicies.AdminOnly)]
public sealed class AdminBookingsController(IBookingService bookingService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<BookingResponse>>> GetAll(
        [FromQuery] int page,
        [FromQuery] int pageSize,
        CancellationToken cancellationToken)
    {
        var bookings = await bookingService.GetAllPageAsync(
            page == 0 ? 1 : page,
            pageSize == 0 ? 100 : pageSize,
            cancellationToken);

        return Ok(bookings);
    }
}
