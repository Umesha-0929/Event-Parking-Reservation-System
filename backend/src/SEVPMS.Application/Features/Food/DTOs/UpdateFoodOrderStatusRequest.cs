namespace SEVPMS.Application.Features.Food.DTOs;

public sealed class UpdateFoodOrderStatusRequest
{
    public string NewStatus { get; set; } = string.Empty;

    public string Note { get; set; } = string.Empty;
}