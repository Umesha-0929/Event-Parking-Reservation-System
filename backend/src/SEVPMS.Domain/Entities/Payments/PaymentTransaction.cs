using SEVPMS.Domain.Common;
using SEVPMS.Domain.Enums;

namespace SEVPMS.Domain.Entities.Payments;

public sealed class PaymentTransaction : BaseEntity
{
    public Guid PaymentId { get; set; }
    public Guid BookingId { get; set; }
    public Guid CustomerUserId { get; set; }
    public string Type { get; set; } = string.Empty;
    public string ProviderReference { get; set; } = string.Empty;
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "LKR";
    public string? PayloadHash { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
