using SEVPMS.Domain.Common;

namespace SEVPMS.Domain.Entities.Food;

public sealed class MenuItem : AuditableEntity
{
    public Guid VendorId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public string Currency { get; set; } = string.Empty;

    public bool IsAvailable { get; set; }

    public string ImageUrl { get; set; } = string.Empty;
}