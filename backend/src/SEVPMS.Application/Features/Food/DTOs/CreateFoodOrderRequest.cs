namespace SEVPMS.Application.Features.Food.DTOs;

public sealed class CreateFoodOrderRequest
{
    public Guid EventId { get; set; }

    public Guid EventFoodStallId { get; set; }

    public Guid? BookingId { get; set; }

    public string FulfillmentType { get; set; } = string.Empty;

    public string? SeatLabelSnapshot { get; set; }

    public List<CreateFoodOrderItemRequest> Items { get; set; } = [];
}