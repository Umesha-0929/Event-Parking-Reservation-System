using SEVPMS.Domain.Common;
using SEVPMS.Domain.Enums;
namespace SEVPMS.Domain.Entities.Tickets;
public sealed class Ticket : AuditableEntity
{
    public Guid BookingId { get; set; }
    public Guid EventId { get; set; }
    public Guid? SeatId { get; set; }
    public string TicketNo { get; set; } = string.Empty;
    public string QrTokenHash { get; set; } = string.Empty;
    public TicketStatus Status { get; set; } = TicketStatus.Active;
    public DateTime IssuedAtUtc { get; set; }
    public DateTime? CheckedInAtUtc { get; set; }
}
