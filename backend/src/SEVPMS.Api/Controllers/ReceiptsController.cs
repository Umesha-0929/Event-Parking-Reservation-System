using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SEVPMS.Api.Authorization;
using SEVPMS.Application.Features.Receipts.DTOs;
using SEVPMS.Application.Features.Receipts.Interfaces;

namespace SEVPMS.Api.Controllers;

[ApiController]
[Route("api/receipts")]
[Authorize(Policy = AuthorizationPolicies.CustomerOnly)]
public sealed class ReceiptsController(IReceiptService receiptService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ReceiptResponse>>> GetMine(CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var userId)) return Unauthorized();
        return Ok(await receiptService.GetMineAsync(userId, cancellationToken));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ReceiptResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var userId)) return Unauthorized();
        return Ok(await receiptService.GetByIdAsync(userId, id, cancellationToken));
    }


    private bool TryGetCurrentUserId(out Guid userId)
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(value, out userId);
    }

}
