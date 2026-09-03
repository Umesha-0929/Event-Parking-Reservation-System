namespace SEVPMS.Application.Features.Parking.Validators;

public sealed class ParkingReservationConflictException(
    string message,
    Exception? innerException = null)
    : Exception(message, innerException);