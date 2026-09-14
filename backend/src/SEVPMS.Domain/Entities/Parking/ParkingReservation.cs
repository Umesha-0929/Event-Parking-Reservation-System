using SEVPMS.Domain.Common;

namespace SEVPMS.Domain.Entities.Parking;

public sealed class ParkingReservation : AuditableEntity
{
    public Guid BookingId { get; set; }

    public Guid ParkingSlotId { get; set; }

    public Guid? VehicleId { get; set; }

    public string VehicleRegSnapshot { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public DateTime ReservedAtUtc { get; set; }
}