using SEVPMS.Application.Features.Places.DTOs;

namespace SEVPMS.Application.Features.Places.Interfaces;

public interface IPlaceFinderService
{
    Task<IReadOnlyList<NearbyPlaceDto>> BrowseAsync(
        Guid venueId,
        PlaceFinderRequest request,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<NearbyPlaceDto>> RecommendAsync(
        Guid venueId,
        PlaceFinderRequest request,
        CancellationToken cancellationToken = default);

    Task<NearbyPlaceDto> CreateAsync(
        ManageNearbyPlaceRequest request,
        CancellationToken cancellationToken = default);

    Task<NearbyPlaceDto> UpdateAsync(
        Guid placeId,
        ManageNearbyPlaceRequest request,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        Guid placeId,
        CancellationToken cancellationToken = default);
}
