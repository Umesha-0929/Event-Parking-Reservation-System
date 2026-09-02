using SEVPMS.Domain.Common;
using SEVPMS.Domain.Enums;

namespace SEVPMS.Domain.Entities.Seats;

public sealed class Seat : AuditableEntity
{
    public Guid EventId { get; set; }

    public Guid SeatingLayoutId { get; set; }

    public Guid SectionId { get; set; }

    public Guid? SeatCategoryId { get; set; }

    public string RowLabel { get; set; } = string.Empty;

    public int RowNumber { get; set; }

    public int ColumnNumber { get; set; }

    public string SeatNumber { get; set; } = string.Empty;

    public decimal X { get; set; }

    public decimal Y { get; set; }

    public Guid? TicketTypeId { get; set; }

    public bool IsAccessible { get; set; }

    public SeatStatus Status { get; set; } = SeatStatus.Available;

    public Guid? SeatViewAssetId { get; set; }

    public byte[] RowVersion { get; set; } = Array.Empty<byte>();
}
