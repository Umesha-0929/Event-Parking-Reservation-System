namespace SEVPMS.Application.Features.Places.DTOs;

public sealed class ManageNearbyPlaceRequest
{
    public Guid VenueId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public List<string> Tags { get; set; } = [];
    public List<string> AudienceModes { get; set; } = [];
    public string Address { get; set; } = string.Empty;
    public decimal DistanceKm { get; set; }
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public bool IsOpen { get; set; } = true;
    public string? DirectionsUrl { get; set; }
    public bool IsActive { get; set; } = true;
}
