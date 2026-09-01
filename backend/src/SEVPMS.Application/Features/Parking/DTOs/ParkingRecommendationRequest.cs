namespace SEVPMS.Application.Features.Parking.DTOs;

public sealed class ParkingRecommendationRequest
{
    public Guid VenueId { get; set; }

    public Guid? EventId { get; set; }

    public Guid EntranceNodeId { get; set; }

    public bool RequiresAccessibleParking { get; set; }

    public Guid? SavedVehicleId { get; set; }
}