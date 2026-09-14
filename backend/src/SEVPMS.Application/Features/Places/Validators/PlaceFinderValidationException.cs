namespace SEVPMS.Application.Features.Places.Validators;

public sealed class PlaceFinderValidationException(string message)
    : Exception(message);
