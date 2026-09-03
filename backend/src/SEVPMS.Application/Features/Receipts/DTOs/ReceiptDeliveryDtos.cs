using SEVPMS.Domain.Enums;

namespace SEVPMS.Application.Features.Receipts.DTOs;

public sealed class ReceiptDeliveryResponse
{
    public Guid ReceiptDeliveryId { get; set; }
    public Guid ReceiptId { get; set; }
    public string Channel { get; set; } = string.Empty;
    public string DestinationMasked { get; set; } = string.Empty;
    public ReceiptDeliveryStatus Status { get; set; }
    public int AttemptCount { get; set; }
    public DateTime? LastAttemptAtUtc { get; set; }
    public DateTime? SentAtUtc { get; set; }
    public string? LastError { get; set; }
}
