using SEVPMS.Application.Features.Vehicles.DTOs;

namespace SEVPMS.Application.Features.Vehicles.Validators;

public static class SavedVehicleValidator
{
    public static void Validate(CreateSavedVehicleRequest request)
    {
        ValidateValues(
            request.Nickname,
            request.RegistrationNo,
            request.VehicleType);
    }

    public static void Validate(UpdateSavedVehicleRequest request)
    {
        ValidateValues(
            request.Nickname,
            request.RegistrationNo,
            request.VehicleType);
    }

    private static void ValidateValues(
        string nickname,
        string registrationNo,
        string vehicleType)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(nickname))
        {
            errors.Add("Nickname is required.");
        }

        if (string.IsNullOrWhiteSpace(registrationNo))
        {
            errors.Add("Registration number is required.");
        }

        if (string.IsNullOrWhiteSpace(vehicleType))
        {
            errors.Add("Vehicle type is required.");
        }

        if (errors.Count > 0)
        {
            throw new SavedVehicleValidationException(errors);
        }
    }
}