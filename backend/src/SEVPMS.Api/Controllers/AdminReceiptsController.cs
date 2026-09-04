using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SEVPMS.Api.Authorization;
using SEVPMS.Application.Features.Receipts.DTOs;
using SEVPMS.Application.Features.Receipts.Interfaces;

namespace SEVPMS.Api.Controllers;

[ApiController]
[Route("api/admin/receipts")]
[Authorize(Policy = AuthorizationPolicies.AdminOnly)]
public sealed class AdminReceiptsController(
    IReceiptDeliveryService deliveryService)
    : ControllerBase
{
    [HttpGet("{id:guid}/deliveries")]
    public async Task<ActionResult<IReadOnlyList<ReceiptDeliveryResponse>>>
        GetDeliveries(
            Guid id,
            CancellationToken cancellationToken)
    {
        return Ok(
            await deliveryService.GetForAdminAsync(
                id,
                cancellationToken));
    }

    [HttpPost("{id:guid}/deliveries/retry")]
    public async Task<ActionResult<IReadOnlyList<ReceiptDeliveryResponse>>>
        RetryDelivery(
            Guid id,
            CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(
                out var adminUserId))
        {
            return Unauthorized();
        }

        return Ok(
            await deliveryService.RetryForAdminAsync(
                adminUserId,
                id,
                cancellationToken));
    }

    private bool TryGetCurrentUserId(
        out Guid userId)
    {
        return Guid.TryParse(
            User.FindFirstValue(
                ClaimTypes.NameIdentifier),
            out userId);
    }
}