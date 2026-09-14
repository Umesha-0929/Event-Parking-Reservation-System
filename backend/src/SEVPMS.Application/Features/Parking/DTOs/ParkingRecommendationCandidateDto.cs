namespace SEVPMS.Application.Features.Parking.DTOs;

public sealed class ParkingRecommendationCandidateDto
{
    public Guid ParkingSlotId { get; set; }

    public Guid ParkingZoneId { get; set; }

    public string SlotCode { get; set; } = string.Empty;

    public decimal DistanceCost { get; set; }

    public bool IsAvailable { get; set; }

    public bool IsAccessible { get; set; }

    public bool IsVehicleSuitable { get; set; }
}