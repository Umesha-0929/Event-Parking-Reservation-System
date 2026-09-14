namespace SEVPMS.Application.Features.Vehicles.Validators;

public sealed class SavedVehicleValidationException(
    IReadOnlyList<string> errors) : Exception("Saved vehicle validation failed.")
{
    public IReadOnlyList<string> Errors { get; } = errors;
}
