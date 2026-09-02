using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SEVPMS.Api.Authorization;
using SEVPMS.Application.Features.Payments.DTOs;
using SEVPMS.Application.Features.Payments.Interfaces;

namespace SEVPMS.Api.Controllers;

[ApiController]
[Route("api/payments")]
public sealed class PaymentsController(IPaymentService paymentService) : ControllerBase
{
    [Authorize(Policy = AuthorizationPolicies.CustomerOnly)]
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<PaymentResponse>>> GetMine(CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var userId)) return Unauthorized();
        return Ok(await paymentService.GetMineAsync(userId, cancellationToken));
    }

    [Authorize(Policy = AuthorizationPolicies.CustomerOnly)]
    [HttpPost]
    public async Task<ActionResult<PaymentResponse>> Start([FromBody] StartPaymentRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var userId)) return Unauthorized();
        var response = await paymentService.StartAsync(userId, request, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, response);
    }

    // Development/mock-provider completion endpoint for backend verification.
    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
    [HttpPost("{id:guid}/complete")]
    public async Task<ActionResult<PaymentResponse>> Complete(Guid id, CancellationToken cancellationToken)
        => Ok(await paymentService.CompleteMockAsync(id, cancellationToken));

    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
    [HttpPost("{id:guid}/fail")]
    public async Task<ActionResult<PaymentResponse>> Fail(Guid id, CancellationToken cancellationToken)
        => Ok(await paymentService.FailMockAsync(id, cancellationToken));


    private bool TryGetCurrentUserId(out Guid userId)
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(value, out userId);
    }

}
