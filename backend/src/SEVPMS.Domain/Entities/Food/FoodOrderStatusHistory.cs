using SEVPMS.Domain.Common;

namespace SEVPMS.Domain.Entities.Food;

public sealed class FoodOrderStatusHistory : BaseEntity
{
    public Guid FoodOrderId { get; set; }

    public string OldStatus { get; set; } = string.Empty;

    public string NewStatus { get; set; } = string.Empty;

    public Guid ChangedByUserId { get; set; }

    public DateTime ChangedAtUtc { get; set; } = DateTime.UtcNow;

    public string Note { get; set; } = string.Empty;
}