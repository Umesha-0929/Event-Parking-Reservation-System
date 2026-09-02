using SEVPMS.Domain.Common;

namespace SEVPMS.Domain.Entities.Parking;

public sealed class ParkingZone : AuditableEntity
{
    public Guid VenueId { get; set; }

    public Guid? EventId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Level { get; set; } = string.Empty;

    public string EntranceName { get; set; } = string.Empty;
}