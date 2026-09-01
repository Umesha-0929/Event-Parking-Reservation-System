namespace SEVPMS.Application.Features.Parking.DTOs;

public sealed class CreateParkingReservationRequest
{
    public Guid BookingId { get; set; }

    public Guid ParkingSlotId { get; set; }

    public Guid? VehicleId { get; set; }

    public string VehicleRegistration { get; set; } = string.Empty;
}