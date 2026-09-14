using SEVPMS.Domain.Common;

namespace SEVPMS.Domain.Entities.Vehicles;

public sealed class SavedVehicle : AuditableEntity
{
    public Guid UserId { get; set; }

    public string Nickname { get; set; } = string.Empty;

    public string RegistrationNo { get; set; } = string.Empty;

    public string VehicleType { get; set; } = string.Empty;

    public bool IsDefault { get; set; }
}