namespace SEVPMS.Application.Features.Vehicles.DTOs;

public sealed class CreateSavedVehicleRequest
{
    public string Nickname { get; set; } = string.Empty;
    public string RegistrationNo { get; set; } = string.Empty;
    public string VehicleType { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
}