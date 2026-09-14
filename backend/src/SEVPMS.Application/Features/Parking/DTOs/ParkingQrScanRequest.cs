namespace SEVPMS.Application.Features.Parking.DTOs;

public sealed class ParkingQrScanRequest
{
    public string ParkingPassCode { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
}
