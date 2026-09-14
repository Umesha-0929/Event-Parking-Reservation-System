namespace SEVPMS.Application.Features.Parking.DTOs;

public sealed class UpsertParkingZoneRequest
{
    public Guid VenueId { get; set; }

    public Guid? EventId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Level { get; set; } = string.Empty;

    public string EntranceName { get; set; } = string.Empty;
}