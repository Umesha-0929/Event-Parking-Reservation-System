using SEVPMS.Domain.Common;

namespace SEVPMS.Domain.Entities.Venues;

public sealed class VenueMedia : AuditableEntity
{
    public Guid VenueId { get; set; }
    public string Url { get; set; } = string.Empty;
    public string Type { get; set; } = "Photo";
    public int SortOrder { get; set; }
}
