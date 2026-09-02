namespace SEVPMS.Application.Features.Food.DTOs;

public sealed class CreateFoodOrderItemRequest
{
    public Guid MenuItemId { get; set; }

    public int Quantity { get; set; }
}
