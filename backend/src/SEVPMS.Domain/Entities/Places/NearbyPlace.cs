using SEVPMS.Domain.Common;

namespace SEVPMS.Domain.Entities.Places;

public sealed class NearbyPlace : AuditableEntity
{
    public Guid VenueId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string TagsCsv { get; set; } = string.Empty;
    public string AudienceModesCsv { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public decimal DistanceKm { get; set; }
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public bool IsOpen { get; set; } = true;
    public string? DirectionsUrl { get; set; }
    public bool IsActive { get; set; } = true;
}
