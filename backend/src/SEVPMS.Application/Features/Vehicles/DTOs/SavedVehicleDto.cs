namespace SEVPMS.Application.Features.Vehicles.DTOs;

public sealed class SavedVehicleDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Nickname { get; set; } = string.Empty;
    public string RegistrationNo { get; set; } = string.Empty;
    public string VehicleType { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
}