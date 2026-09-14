namespace SEVPMS.Application.Features.Parking.DTOs;

public sealed class ParkingNodeDto
{
    public Guid Id { get; set; }

    public Guid VenueId { get; set; }

    public Guid? LayoutId { get; set; }

    public string NodeCode { get; set; } = string.Empty;

    public decimal X { get; set; }

    public decimal Y { get; set; }

    public string NodeType { get; set; } = string.Empty;
}