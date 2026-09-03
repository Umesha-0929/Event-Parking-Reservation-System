namespace SEVPMS.Application.Features.Parking.Validators;

public sealed class ParkingReservationValidationException(string message)
    : Exception(message);
