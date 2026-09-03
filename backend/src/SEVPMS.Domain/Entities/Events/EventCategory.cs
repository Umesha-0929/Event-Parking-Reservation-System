using SEVPMS.Domain.Common;

namespace SEVPMS.Domain.Entities.Events;

public sealed class EventCategory : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}
