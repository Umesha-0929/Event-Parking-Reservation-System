using System.ComponentModel.DataAnnotations.Schema;
using SEVPMS.Domain.Common;
using SEVPMS.Domain.Enums;

namespace SEVPMS.Domain.Entities.Events;

public sealed class Event : AuditableEntity
{
    public Guid OrganizerUserId { get; set; }

    public Guid VenueId { get; set; }

    public Guid CategoryId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    // Compatibility-only property.
    // Category name is no longer stored directly in Events table.
    [NotMapped]
    public string Category { get; set; } = string.Empty;

    public EventCategory? CategoryEntity { get; set; }

    public DateTime StartAtUtc { get; set; }

    public DateTime EndAtUtc { get; set; }

    public EventStatus Status { get; set; } =
        EventStatus.Draft;
}