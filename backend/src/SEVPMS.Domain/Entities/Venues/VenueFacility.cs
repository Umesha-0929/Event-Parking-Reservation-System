using SEVPMS.Domain.Common;

namespace SEVPMS.Domain.Entities.Venues;

public sealed class VenueFacility : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}
