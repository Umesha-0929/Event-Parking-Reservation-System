using SEVPMS.Application.Features.VenueRentals.DTOs;

namespace SEVPMS.Application.Features.VenueRentals.Interfaces;

public interface IVenueRentalService
{
    Task<IReadOnlyList<VenueRentalResponse>> GetMineAsync(Guid organizerUserId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<VenueRentalResponse>> GetIncomingAsync(Guid venueOwnerUserId, CancellationToken cancellationToken = default);
    Task<VenueRentalResponse> CreateAsync(Guid organizerUserId, CreateVenueRentalRequest request, CancellationToken cancellationToken = default);
    Task<VenueRentalResponse> UpdateStatusAsync(Guid venueOwnerUserId, Guid rentalId, UpdateVenueRentalStatusRequest request, CancellationToken cancellationToken = default);
    Task<VenueRentalResponse> CancelAsync(Guid organizerUserId, Guid rentalId, CancellationToken cancellationToken = default);
}
