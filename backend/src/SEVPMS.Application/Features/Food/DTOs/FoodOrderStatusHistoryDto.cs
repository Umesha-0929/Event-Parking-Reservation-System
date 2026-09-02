namespace SEVPMS.Application.Features.Food.DTOs;

public sealed class FoodOrderStatusHistoryDto
{
    public Guid Id { get; set; }

    public Guid FoodOrderId { get; set; }

    public string OldStatus { get; set; } = string.Empty;

    public string NewStatus { get; set; } = string.Empty;

    public Guid ChangedByUserId { get; set; }

    public DateTime ChangedAtUtc { get; set; }

    public string Note { get; set; } = string.Empty;
}