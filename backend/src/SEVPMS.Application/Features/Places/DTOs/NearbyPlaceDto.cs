namespace SEVPMS.Application.Features.Places.DTOs;

public sealed class NearbyPlaceDto
{
    public Guid Id { get; set; }
    public Guid VenueId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public IReadOnlyList<string> Tags { get; set; } = [];
    public IReadOnlyList<string> AudienceModes { get; set; } = [];
    public string Address { get; set; } = string.Empty;
    public decimal DistanceKm { get; set; }
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public bool IsOpen { get; set; }
    public string? DirectionsUrl { get; set; }
    public string RecommendationReason { get; set; } = string.Empty;
}
