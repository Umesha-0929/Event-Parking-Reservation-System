using SEVPMS.Application.Features.Vehicles.DTOs;
using SEVPMS.Application.Features.Vehicles.Validators;
using Xunit;

namespace SEVPMS.UnitTests.Vehicles;

public sealed class SavedVehicleValidatorTests
{
    [Fact]
    public void Validate_CreateRequest_WithValidValues_DoesNotThrow()
    {
        var request = new CreateSavedVehicleRequest
        {
            Nickname = "My Car",
            RegistrationNo = "WP CAB 1234",
            VehicleType = "Car",
            IsDefault = true
        };

        var exception = Record.Exception(
            () => SavedVehicleValidator.Validate(request));

        Assert.Null(exception);
    }

    [Fact]
    public void Validate_CreateRequest_WithMissingRequiredValues_ThrowsValidationException()
    {
        var request = new CreateSavedVehicleRequest
        {
            Nickname = " ",
            RegistrationNo = "",
            VehicleType = " "
        };

        var exception = Assert.Throws<SavedVehicleValidationException>(
            () => SavedVehicleValidator.Validate(request));

        Assert.Contains("Nickname is required.", exception.Errors);
        Assert.Contains("Registration number is required.", exception.Errors);
        Assert.Contains("Vehicle type is required.", exception.Errors);
    }

    [Fact]
    public void Validate_UpdateRequest_WithValidValues_DoesNotThrow()
    {
        var request = new UpdateSavedVehicleRequest
        {
            Nickname = "Family Car",
            RegistrationNo = "WP ABC 5678",
            VehicleType = "Car",
            IsDefault = false
        };

        var exception = Record.Exception(
            () => SavedVehicleValidator.Validate(request));

        Assert.Null(exception);
    }

    [Fact]
    public void Validate_UpdateRequest_WithMissingRequiredValues_ThrowsValidationException()
    {
        var request = new UpdateSavedVehicleRequest
        {
            Nickname = "",
            RegistrationNo = " ",
            VehicleType = ""
        };

        var exception = Assert.Throws<SavedVehicleValidationException>(
            () => SavedVehicleValidator.Validate(request));

        Assert.Equal(3, exception.Errors.Count);
    }
}