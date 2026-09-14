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
    IReceiptService receiptService,
    IReceiptDeliveryService deliveryService)
    : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ReceiptResponse>>> GetRecent(
        [FromQuery] int take = 100,
        CancellationToken cancellationToken = default)
        => Ok(await receiptService.GetRecentAdminAsync(take, cancellationToken));

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