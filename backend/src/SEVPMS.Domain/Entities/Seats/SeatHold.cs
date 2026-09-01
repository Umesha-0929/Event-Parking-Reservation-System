using SEVPMS.Domain.Common;
using SEVPMS.Domain.Enums;
namespace SEVPMS.Domain.Entities.Seats;
public sealed class SeatHold : AuditableEntity
{
    public Guid EventId { get; set; }
    public Guid SeatId { get; set; }
    public Guid UserId { get; set; }
    public string HoldToken { get; set; } = string.Empty;
    public DateTime ExpiresAtUtc { get; set; }
    public SeatHoldStatus Status { get; set; } = SeatHoldStatus.Active;
}
