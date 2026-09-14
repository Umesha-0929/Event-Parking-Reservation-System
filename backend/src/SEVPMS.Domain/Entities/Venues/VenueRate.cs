using SEVPMS.Domain.Common;

namespace SEVPMS.Domain.Entities.Venues;

public sealed class VenueRate : AuditableEntity
{
    public Guid VenueId { get; set; }
    public string RateType { get; set; } = "Hourly";
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "LKR";
    public DateTime? ValidFromUtc { get; set; }
    public DateTime? ValidToUtc { get; set; }
    public bool IsActive { get; set; } = true;
}
