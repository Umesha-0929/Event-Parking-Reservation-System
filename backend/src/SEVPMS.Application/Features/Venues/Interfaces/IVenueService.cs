using SEVPMS.Application.Features.Venues.DTOs;

namespace SEVPMS.Application.Features.Venues.Interfaces;

public interface IVenueService
{
    Task<IReadOnlyList<VenueResponse>> GetActiveVenuesAsync(
        CancellationToken cancellationToken = default);

    async Task<IReadOnlyList<VenueResponse>> GetActiveVenuesPageAsync(
        int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var all = await GetActiveVenuesAsync(cancellationToken);
        var safePage = Math.Max(page, 1);
        var safePageSize = Math.Clamp(pageSize, 1, 200);
        return all.Skip((safePage - 1) * safePageSize).Take(safePageSize).ToArray();
    }

    Task<VenueResponse> GetByIdAsync(
        Guid venueId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<VenueResponse>> GetMyVenuesAsync(
        Guid ownerUserId,
        CancellationToken cancellationToken = default);

    async Task<IReadOnlyList<VenueResponse>> GetMyVenuesPageAsync(
        Guid ownerUserId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var all = await GetMyVenuesAsync(ownerUserId, cancellationToken);
        var safePage = Math.Max(page, 1);
        var safePageSize = Math.Clamp(pageSize, 1, 200);
        return all.Skip((safePage - 1) * safePageSize).Take(safePageSize).ToArray();
    }

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
    Task DeletePermanentAsync(Guid ownerUserId, Guid venueId, CancellationToken cancellationToken = default);

}