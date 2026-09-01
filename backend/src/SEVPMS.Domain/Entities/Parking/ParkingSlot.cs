using SEVPMS.Domain.Common;

namespace SEVPMS.Domain.Entities.Parking;

public sealed class ParkingSlot : AuditableEntity
{
    public Guid ParkingZoneId { get; set; }

    public Guid? EventId { get; set; }

    public string SlotCode { get; set; } = string.Empty;

    public decimal X { get; set; }

    public decimal Y { get; set; }

    public bool IsAccessible { get; set; }

    public string Status { get; set; } = string.Empty;
}