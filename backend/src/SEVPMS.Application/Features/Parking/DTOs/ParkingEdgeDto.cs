namespace SEVPMS.Application.Features.Parking.DTOs;

public sealed class ParkingEdgeDto
{
    public Guid Id { get; set; }

    public Guid VenueId { get; set; }

    public Guid FromNodeId { get; set; }

    public Guid ToNodeId { get; set; }

    public decimal Cost { get; set; }

    public bool IsBidirectional { get; set; }

    public bool IsAccessible { get; set; }

    public bool IsBlocked { get; set; }
}