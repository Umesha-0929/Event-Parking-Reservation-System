using SEVPMS.Domain.Common;
using SEVPMS.Domain.Enums;

namespace SEVPMS.Domain.Entities.VenueRentals;

public sealed class VenueRentalRequest : AuditableEntity
{
    public Guid OrganizerUserId { get; set; }
    public Guid VenueId { get; set; }
    public DateTime StartAtUtc { get; set; }
    public DateTime EndAtUtc { get; set; }
    public string Purpose { get; set; } = string.Empty;
    public decimal OfferedAmount { get; set; }
    public RentalRequestStatus Status { get; set; } = RentalRequestStatus.Pending;
    public string? OwnerMessage { get; set; }
}
