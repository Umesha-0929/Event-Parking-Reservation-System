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
    public string QrPayload { get; set; } = string.Empty;
    public PaymentStatus Status { get; set; }
    public DateTime? PaidAtUtc { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}

public sealed class SandboxPaymentCallbackRequest
{
    public Guid PaymentId { get; set; }
    public string ProviderReference { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "LKR";
    public long TimestampUnix { get; set; }
    public string Signature { get; set; } = string.Empty;
}


public sealed class PayHereNotifyRequest
{
    public string MerchantId { get; set; } = string.Empty;
    public string OrderId { get; set; } = string.Empty;
    public string PaymentId { get; set; } = string.Empty;
    public string PayHereAmount { get; set; } = string.Empty;
    public string PayHereCurrency { get; set; } = string.Empty;
    public string StatusCode { get; set; } = string.Empty;
    public string Md5Sig { get; set; } = string.Empty;
}

public sealed class PayHereCustomerDetails
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
}

public sealed class PayHereCheckoutResponse
{
    public Guid PaymentId { get; set; }
    public string CheckoutUrl { get; set; } = string.Empty;
    public string MerchantId { get; set; } = string.Empty;
    public string ReturnUrl { get; set; } = string.Empty;
    public string CancelUrl { get; set; } = string.Empty;
    public string NotifyUrl { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string OrderId { get; set; } = string.Empty;
    public string Items { get; set; } = string.Empty;
    public string Amount { get; set; } = string.Empty;
    public string Currency { get; set; } = "LKR";
    public string Hash { get; set; } = string.Empty;
}

public sealed class ManualPaymentProofRequest
{
    public string ProofUrl { get; set; } = string.Empty;
    public string? Reference { get; set; }
}

public sealed class ManualPaymentReviewResponse
{
    public Guid PaymentId { get; set; }
    public Guid BookingId { get; set; }
    public Guid EventId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string ProofUrl { get; set; } = string.Empty;
    public DateTime SubmittedAtUtc { get; set; }
}

public sealed class PaymentTransactionResponse
{
    public Guid PaymentTransactionId { get; set; }
    public Guid PaymentId { get; set; }
    public string Type { get; set; } = string.Empty;
    public string ProviderReference { get; set; } = string.Empty;
    public PaymentStatus Status { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
}

public sealed class RefundResponse
{
    public Guid RefundId { get; set; }
    public Guid PaymentId { get; set; }
    public Guid BookingId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public string RefundReference { get; set; } = string.Empty;
    public RefundStatus Status { get; set; }
    public DateTime? RefundedAtUtc { get; set; }
}
