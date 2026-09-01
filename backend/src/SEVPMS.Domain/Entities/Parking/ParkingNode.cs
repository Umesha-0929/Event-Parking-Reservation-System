using SEVPMS.Domain.Common;

namespace SEVPMS.Domain.Entities.Parking;

public sealed class ParkingNode : AuditableEntity
{
    public Guid VenueId { get; set; }

    public Guid? LayoutId { get; set; }

    public string NodeCode { get; set; } = string.Empty;

    public decimal X { get; set; }

    public decimal Y { get; set; }

    public string NodeType { get; set; } = string.Empty;
}