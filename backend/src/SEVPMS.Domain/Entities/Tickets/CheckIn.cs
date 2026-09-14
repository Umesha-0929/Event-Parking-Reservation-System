using SEVPMS.Domain.Common;
using SEVPMS.Domain.Enums;
namespace SEVPMS.Domain.Entities.Tickets;
public sealed class CheckIn : AuditableEntity
{
    public Guid TicketId { get; set; }
    public Guid EventId { get; set; }
    public Guid ScannedByUserId { get; set; }
    public string Gate { get; set; } = string.Empty;
    public DateTime ScannedAtUtc { get; set; }
    public CheckInResult Result { get; set; }
    public string? Detail { get; set; }
}
