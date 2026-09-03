using SEVPMS.Domain.Common;
using SEVPMS.Domain.Enums;

namespace SEVPMS.Domain.Entities.Venues;

public sealed class VenueAvailability : AuditableEntity
{
    public Guid VenueId { get; set; }
    public DateTime StartAtUtc { get; set; }
    public DateTime EndAtUtc { get; set; }
    public VenueAvailabilityType Type { get; set; } = VenueAvailabilityType.Available;
    public string? Notes { get; set; }
}
