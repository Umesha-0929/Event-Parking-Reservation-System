namespace SEVPMS.Application.Features.Food.DTOs;

public sealed class FoodOrderDto
{
    public Guid Id { get; set; }

    public string OrderNo { get; set; } = string.Empty;

    public Guid CustomerUserId { get; set; }

    public Guid EventId { get; set; }

    public Guid EventFoodStallId { get; set; }

    public Guid? BookingId { get; set; }

    public string Status { get; set; } = string.Empty;

    public string FulfillmentType { get; set; } = string.Empty;

    public string? SeatLabelSnapshot { get; set; }

    public decimal Total { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public IReadOnlyList<FoodOrderItemDto> Items { get; set; } = [];
}