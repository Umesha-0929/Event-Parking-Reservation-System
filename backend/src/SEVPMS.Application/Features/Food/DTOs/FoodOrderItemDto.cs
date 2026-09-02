namespace SEVPMS.Application.Features.Food.DTOs;

public sealed class FoodOrderItemDto
{
    public Guid Id { get; set; }

    public Guid MenuItemId { get; set; }

    public string ItemNameSnapshot { get; set; } = string.Empty;

    public decimal UnitPrice { get; set; }

    public int Quantity { get; set; }

    public decimal LineTotal { get; set; }
}