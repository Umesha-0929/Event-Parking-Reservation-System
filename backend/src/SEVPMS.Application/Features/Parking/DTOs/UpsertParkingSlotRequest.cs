namespace SEVPMS.Application.Features.Parking.DTOs;

public sealed class UpsertParkingSlotRequest
{
    public Guid ParkingZoneId { get; set; }

    public Guid? EventId { get; set; }

    public string SlotCode { get; set; } = string.Empty;

    public decimal X { get; set; }

    public decimal Y { get; set; }

    public bool IsAccessible { get; set; }

    public string Status { get; set; } = "Available";
}