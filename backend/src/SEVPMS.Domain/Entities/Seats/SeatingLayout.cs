using SEVPMS.Domain.Common;
using SEVPMS.Domain.Enums;

namespace SEVPMS.Domain.Entities.Seats;

public sealed class SeatingLayout : AuditableEntity
{
    public Guid EventId { get; set; }

    public StageType StageType { get; set; }

    public int RowCount { get; set; }

    public int ColumnCount { get; set; }

    public decimal CanvasWidth { get; set; } = 1200;

    public decimal CanvasHeight { get; set; } = 800;

    public decimal StageX { get; set; }

    public decimal StageY { get; set; }

    public decimal StageWidth { get; set; }

    public decimal StageHeight { get; set; }

    public bool IsPublished { get; set; }

    public DateTime? PublishedAtUtc { get; set; }
}
