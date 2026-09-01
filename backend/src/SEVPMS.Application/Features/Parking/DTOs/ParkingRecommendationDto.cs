namespace SEVPMS.Application.Features.Parking.DTOs;

public sealed class ParkingRecommendationDto
{
    public Guid ParkingSlotId { get; set; }

    public Guid ParkingZoneId { get; set; }

    public string SlotCode { get; set; } = string.Empty;

    public decimal DistanceCost { get; set; }

    public bool IsAccessible { get; set; }

    public string Reason { get; set; } = string.Empty;
}