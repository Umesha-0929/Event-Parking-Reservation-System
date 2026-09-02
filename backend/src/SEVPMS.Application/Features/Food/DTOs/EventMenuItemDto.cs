namespace SEVPMS.Application.Features.Food.DTOs;

public sealed class EventMenuItemDto
{
    public Guid Id { get; set; }

    public Guid EventFoodStallId { get; set; }

    public Guid MenuItemId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public string Currency { get; set; } = string.Empty;

    public bool IsAvailable { get; set; }

    public string ImageUrl { get; set; } = string.Empty;
}