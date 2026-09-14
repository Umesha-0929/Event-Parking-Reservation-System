using SEVPMS.Domain.Common;

namespace SEVPMS.Domain.Entities.Seats;

public sealed class SeatSection : AuditableEntity
{
    public Guid EventId { get; set; }

    public Guid SeatingLayoutId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Code { get; set; } = string.Empty;

    public int RowCount { get; set; }

    public int ColumnCount { get; set; }

    public decimal X { get; set; }

    public decimal Y { get; set; }

    public decimal Width { get; set; }

    public decimal Height { get; set; }

    public int DisplayOrder { get; set; }

    public bool IsAccessibleSection { get; set; }

    public bool IsEnabled { get; set; } = true;
}
