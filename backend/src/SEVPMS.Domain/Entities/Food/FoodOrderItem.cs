using SEVPMS.Domain.Common;

namespace SEVPMS.Domain.Entities.Food;

public sealed class FoodOrderItem : BaseEntity
{
    public Guid FoodOrderId { get; set; }

    public Guid MenuItemId { get; set; }

    public string ItemNameSnapshot { get; set; } = string.Empty;

    public decimal UnitPrice { get; set; }

    public int Quantity { get; set; }

    public decimal LineTotal { get; set; }
}
