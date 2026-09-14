using SEVPMS.Domain.Common;

namespace SEVPMS.Domain.Entities.Reviews;

public sealed class EventReview : AuditableEntity
{
    public Guid EventId { get; set; }

    public Guid CustomerUserId { get; set; }

    public Guid BookingId { get; set; }

    public int Rating { get; set; }

    public string? Comment { get; set; }
}