using SEVPMS.Domain.Common;

namespace SEVPMS.Domain.Entities.Parking;

public sealed class ParkingEdge : AuditableEntity
{
    public Guid VenueId { get; set; }

    public Guid FromNodeId { get; set; }

    public Guid ToNodeId { get; set; }

    public decimal Cost { get; set; }

    public bool IsBidirectional { get; set; }

    public bool IsAccessible { get; set; }

    public bool IsBlocked { get; set; }
}