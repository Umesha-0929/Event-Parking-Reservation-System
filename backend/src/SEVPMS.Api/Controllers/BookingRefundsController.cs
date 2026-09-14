using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SEVPMS.Api.Authorization;
using SEVPMS.Application.Features.Bookings.Interfaces;
using SEVPMS.Application.Features.Payments.DTOs;

namespace SEVPMS.Api.Controllers;

[ApiController]
[Route("api/bookings")]
[Authorize(Policy = AuthorizationPolicies.CustomerOnly)]
public sealed class BookingRefundsController(
    IConfirmedBookingCancellationService service)
    : ControllerBase
{
    public sealed class ConfirmedCancellationRequest
    {
        public string Reason { get; set; } = string.Empty;
    }

    [HttpPost("{id:guid}/cancel-confirmed")]
    public async Task<ActionResult<RefundResponse>> CancelConfirmed(
        Guid id,
        [FromBody] ConfirmedCancellationRequest request,
        CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
            return Unauthorized();

        return Ok(await service.CancelAndRefundAsync(
            userId,
            id,
            request.Reason,
            cancellationToken));
    }
}
