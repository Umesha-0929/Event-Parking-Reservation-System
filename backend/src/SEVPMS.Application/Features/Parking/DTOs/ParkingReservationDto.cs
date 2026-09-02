namespace SEVPMS.Application.Features.Parking.DTOs;

public sealed class ParkingReservationDto
{
    public Guid Id { get; set; }

    public Guid BookingId { get; set; }

    public Guid ParkingSlotId { get; set; }

    public Guid? VehicleId { get; set; }

    public string VehicleRegSnapshot { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public DateTime ReservedAtUtc { get; set; }
}