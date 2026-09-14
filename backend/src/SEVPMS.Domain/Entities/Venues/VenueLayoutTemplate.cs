using SEVPMS.Domain.Common;

namespace SEVPMS.Domain.Entities.Venues;

/// <summary>
/// Shared venue-level layout metadata only.
/// Seat generation, seat inventory, seat categories, seat holds and
/// ticket/check-in logic remain owned by Klegar.
/// </summary>
public sealed class VenueLayoutTemplate : AuditableEntity
{
    public Guid VenueId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Version { get; set; } = 1;
    public string LayoutJson { get; set; } = "{}";
    public bool IsActive { get; set; } = true;
}
