using SEVPMS.Domain.Enums;

namespace SEVPMS.Application.Features.Payments.DTOs;

public sealed class StartPaymentRequest
{
    public Guid BookingId { get; set; }
}

public sealed class PaymentResponse
{
    public Guid PaymentId { get; set; }
    public Guid BookingId { get; set; }
    public Guid CustomerUserId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string Provider { get; set; } = string.Empty;
    public string CheckoutReference { get; set; } = string.Empty;
    public PaymentStatus Status { get; set; }
    public DateTime? PaidAtUtc { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}
