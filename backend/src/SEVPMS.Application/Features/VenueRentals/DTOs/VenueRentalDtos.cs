using SEVPMS.Domain.Enums;

namespace SEVPMS.Application.Features.VenueRentals.DTOs;

public sealed class CreateVenueRentalRequest
{
    public Guid VenueId { get; set; }
    public DateTime StartAtUtc { get; set; }
    public DateTime EndAtUtc { get; set; }
    public string Purpose { get; set; } = string.Empty;
    public decimal OfferedAmount { get; set; }
}

public sealed class UpdateVenueRentalStatusRequest
{
    public RentalRequestStatus Status { get; set; }
    public string? OwnerMessage { get; set; }
}

public sealed class VenueRentalResponse
{
    public Guid RentalRequestId { get; set; }
    public Guid OrganizerUserId { get; set; }
    public Guid VenueId { get; set; }
    public DateTime StartAtUtc { get; set; }
    public DateTime EndAtUtc { get; set; }
    public string Purpose { get; set; } = string.Empty;
    public decimal OfferedAmount { get; set; }
    public RentalRequestStatus Status { get; set; }
    public string? OwnerMessage { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
}
