using SEVPMS.Domain.Common;

namespace SEVPMS.Domain.Entities.Food;

public sealed class EventMenuItem : AuditableEntity
{
    public Guid EventFoodStallId { get; set; }

    public Guid MenuItemId { get; set; }

    public decimal? EventPriceOverride { get; set; }

    public bool IsAvailable { get; set; }
}