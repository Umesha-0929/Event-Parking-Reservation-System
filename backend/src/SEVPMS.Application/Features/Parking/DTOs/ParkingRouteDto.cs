namespace SEVPMS.Application.Features.Parking.DTOs;

public sealed class ParkingRouteDto
{
    public Guid StartNodeId { get; set; }

    public Guid EndNodeId { get; set; }

    public decimal TotalCost { get; set; }

    public IReadOnlyList<ParkingNodeDto> Nodes { get; set; }
        = [];
}
