using SEVPMS.Domain.Common;
using SEVPMS.Domain.Enums;

namespace SEVPMS.Domain.Entities.Receipts;

public sealed class ReceiptDelivery : AuditableEntity
{
    public Guid ReceiptId { get; set; }
    public Guid CustomerUserId { get; set; }
    public string Channel { get; set; } = string.Empty;
    public string DestinationMasked { get; set; } = string.Empty;
    public ReceiptDeliveryStatus Status { get; set; } = ReceiptDeliveryStatus.Pending;
    public int AttemptCount { get; set; }
    public DateTime? LastAttemptAtUtc { get; set; }
    public DateTime? SentAtUtc { get; set; }
    public string? LastError { get; set; }
}
