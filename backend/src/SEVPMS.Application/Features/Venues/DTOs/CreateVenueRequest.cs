namespace SEVPMS.Application.Features.Venues.DTOs;

public sealed class CreateVenueRequest
{
    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string AddressLine1 { get; set; } = string.Empty;

    public string? AddressLine2 { get; set; }

    public string City { get; set; } = string.Empty;

    public string District { get; set; } = string.Empty;

    public string Country { get; set; } = "Sri Lanka";

    public decimal? Latitude { get; set; }

    public decimal? Longitude { get; set; }

    public int Capacity { get; set; }

    public string? ContactPhone { get; set; }

    public string? ContactEmail { get; set; }
}