namespace SEVPMS.Application.Features.Places.DTOs;

public sealed class PlaceFinderRequest
{
    public string? AudienceMode { get; set; }
    public string? Category { get; set; }
    public decimal? MaxDistanceKm { get; set; }
    public bool IncludeClosed { get; set; }
}
