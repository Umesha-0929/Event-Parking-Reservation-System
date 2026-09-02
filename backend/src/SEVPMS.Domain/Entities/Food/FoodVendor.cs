using SEVPMS.Domain.Common;

namespace SEVPMS.Domain.Entities.Food;

public sealed class FoodVendor : AuditableEntity
{
    public Guid? OwnerUserId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;
}