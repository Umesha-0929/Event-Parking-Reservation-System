using SEVPMS.Domain.Common;

namespace SEVPMS.Domain.Entities.Seats;

public sealed class SeatCategory : AuditableEntity
{
    public Guid EventId { get; set; }

    public Guid SeatingLayoutId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Code { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public int DisplayOrder { get; set; }

    public bool IsActive { get; set; } = true;
}
