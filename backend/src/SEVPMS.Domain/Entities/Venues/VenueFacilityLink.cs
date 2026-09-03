using SEVPMS.Domain.Common;

namespace SEVPMS.Domain.Entities.Venues;

public sealed class VenueFacilityLink : BaseEntity
{
    public Guid VenueId { get; set; }
    public Guid FacilityId { get; set; }
    public string? Notes { get; set; }
}
