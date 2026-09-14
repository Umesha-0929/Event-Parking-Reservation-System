using SEVPMS.Domain.Common;

namespace SEVPMS.Domain.Entities.Food;

public sealed class EventFoodStall : AuditableEntity
{
    public Guid EventId { get; set; }

    public Guid VendorId { get; set; }

    public Guid? HallLayoutElementId { get; set; }

    public string StallName { get; set; } = string.Empty;

    public bool IsActive { get; set; }

    public DateTime OpensAtUtc { get; set; }

    public DateTime ClosesAtUtc { get; set; }
}