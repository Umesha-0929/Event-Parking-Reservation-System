using SEVPMS.Application.Features.Venues.DTOs;

namespace SEVPMS.Application.Features.Venues.Interfaces;

public interface IVenueService
{
    Task<IReadOnlyList<VenueResponse>> GetActiveVenuesAsync(
        CancellationToken cancellationToken = default);

    Task<VenueResponse> GetByIdAsync(
        Guid venueId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<VenueResponse>> GetMyVenuesAsync(
        Guid ownerUserId,
        CancellationToken cancellationToken = default);

    Task<VenueResponse> CreateAsync(
        Guid ownerUserId,
        CreateVenueRequest request,
        CancellationToken cancellationToken = default);

    Task<VenueResponse> UpdateAsync(
        Guid ownerUserId,
        Guid venueId,
        UpdateVenueRequest request,
        CancellationToken cancellationToken = default);

    Task DeactivateAsync(
        Guid ownerUserId,
        Guid venueId,
        CancellationToken cancellationToken = default);
}