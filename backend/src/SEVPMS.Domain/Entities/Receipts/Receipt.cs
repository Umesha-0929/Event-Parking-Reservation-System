using SEVPMS.Domain.Common;

namespace SEVPMS.Domain.Entities.Receipts;

public sealed class Receipt : AuditableEntity
{
    public string ReceiptNumber { get; set; } = string.Empty;
    public Guid PaymentId { get; set; }
    public Guid BookingId { get; set; }
    public Guid CustomerUserId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "LKR";
    public DateTime IssuedAtUtc { get; set; } = DateTime.UtcNow;
}
