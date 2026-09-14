using SEVPMS.Domain.Common;
using SEVPMS.Domain.Enums;

namespace SEVPMS.Domain.Entities.Waitlists;

public sealed class WaitlistEntry : AuditableEntity
{
    public Guid EventId { get; set; }

    public Guid CustomerUserId { get; set; }

    public WaitlistStatus Status { get; set; } =
        WaitlistStatus.Waiting;

    public DateTime? EligibleAtUtc { get; set; }

    public DateTime? LeftAtUtc { get; set; }

    public DateTime? ConvertedAtUtc { get; set; }
}