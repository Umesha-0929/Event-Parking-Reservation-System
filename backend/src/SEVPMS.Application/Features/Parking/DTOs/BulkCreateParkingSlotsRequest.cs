namespace SEVPMS.Application.Features.Parking.DTOs;

public sealed class BulkCreateParkingSlotsRequest
{
    public List<UpsertParkingSlotRequest> Slots { get; set; } = [];
}
