namespace SEVPMS.Application.Features.Receipts.DTOs;

public sealed class ReceiptResponse
{
    public Guid ReceiptId { get; set; }
    public string ReceiptNumber { get; set; } = string.Empty;
    public Guid PaymentId { get; set; }
    public Guid BookingId { get; set; }
    public Guid CustomerUserId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public DateTime IssuedAtUtc { get; set; }
}
