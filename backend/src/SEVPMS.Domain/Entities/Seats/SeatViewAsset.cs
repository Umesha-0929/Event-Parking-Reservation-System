using SEVPMS.Domain.Common;
namespace SEVPMS.Domain.Entities.Seats;
public sealed class SeatViewAsset : AuditableEntity
{
    public Guid EventId { get; set; }
    public Guid? SectionId { get; set; }
    public Guid? SeatId { get; set; }
    public string MediaUrl { get; set; } = string.Empty;
    public string ViewerType { get; set; } = "panorama";
    public decimal? DefaultYaw { get; set; }
    public decimal? DefaultPitch { get; set; }
    public decimal? DefaultFov { get; set; }
    public bool IsRepresentative { get; set; } = true;
}
