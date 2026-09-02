using SEVPMS.Domain.Common;
using SEVPMS.Domain.Enums;

namespace SEVPMS.Domain.Entities.Bookings;

public sealed class Booking : AuditableEntity
{
    public string BookingNumber { get; set; } = string.Empty;
    public Guid CustomerUserId { get; set; }
    public Guid EventId { get; set; }
    public string HoldToken { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public BookingStatus Status { get; set; } = BookingStatus.Pending;
    public DateTime? ConfirmedAtUtc { get; set; }
    public DateTime? CancelledAtUtc { get; set; }
}
