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
    public async Task<ActionResult<IReadOnlyList<PaymentResponse>>> GetMine(
        CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var userId)) return Unauthorized();
        return Ok(await paymentService.GetMineAsync(userId, cancellationToken));
    }

    [Authorize(Policy = AuthorizationPolicies.CustomerOnly)]
    [HttpPost]
    public async Task<ActionResult<PaymentResponse>> Start(
        [FromBody] StartPaymentRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var userId)) return Unauthorized();
        return StatusCode(
            StatusCodes.Status201Created,
            await paymentService.StartAsync(userId, request, cancellationToken));
    }


    [Authorize(Policy = AuthorizationPolicies.CustomerOnly)]
    [HttpPost("{id:guid}/payhere-checkout")]
    public async Task<ActionResult<PayHereCheckoutResponse>> PayHereCheckout(
        Guid id,
        CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var userId)) return Unauthorized();
        return Ok(await paymentService.GetPayHereCheckoutAsync(
            userId,
            id,
            cancellationToken));
    }

    [AllowAnonymous]
    [Consumes("application/x-www-form-urlencoded")]
    [HttpPost("payhere/notify")]
    public async Task<ActionResult<PaymentResponse>> PayHereNotify(
        CancellationToken cancellationToken)
    {
        var form = await Request.ReadFormAsync(cancellationToken);

        var request = new PayHereNotifyRequest
        {
            MerchantId = form["merchant_id"].ToString(),
            OrderId = form["order_id"].ToString(),
            PaymentId = form["payment_id"].ToString(),
            PayHereAmount = form["payhere_amount"].ToString(),
            PayHereCurrency = form["payhere_currency"].ToString(),
            StatusCode = form["status_code"].ToString(),
            Md5Sig = form["md5sig"].ToString()
        };

        return Ok(await paymentService.ProcessPayHereNotificationAsync(
            request,
            cancellationToken));
    }

    [Authorize(Policy = AuthorizationPolicies.CustomerOnly)]
    [HttpGet("{id:guid}/transactions")]
    public async Task<ActionResult<IReadOnlyList<PaymentTransactionResponse>>> Transactions(
        Guid id,
        CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var userId)) return Unauthorized();
        return Ok(await paymentService.GetTransactionsAsync(userId, id, cancellationToken));
    }

    [AllowAnonymous]
    [HttpPost("sandbox/callback")]
    public async Task<ActionResult<PaymentResponse>> SandboxCallback(
        [FromBody] SandboxPaymentCallbackRequest request,
        CancellationToken cancellationToken)
        => Ok(await paymentService.ProcessSandboxCallbackAsync(request, cancellationToken));

    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
    [HttpPost("{id:guid}/complete")]
    public async Task<ActionResult<PaymentResponse>> Complete(
        Guid id,
        CancellationToken cancellationToken)
        => Ok(await paymentService.CompleteMockAsync(id, cancellationToken));

    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
    [HttpPost("{id:guid}/fail")]
    public async Task<ActionResult<PaymentResponse>> Fail(
        Guid id,
        CancellationToken cancellationToken)
        => Ok(await paymentService.FailMockAsync(id, cancellationToken));

    private bool TryGetCurrentUserId(out Guid userId)
        => Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out userId);
}
