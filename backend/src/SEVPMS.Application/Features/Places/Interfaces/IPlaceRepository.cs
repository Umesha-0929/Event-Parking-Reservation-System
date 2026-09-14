using SEVPMS.Domain.Entities.Places;

namespace SEVPMS.Application.Features.Places.Interfaces;

public interface IPlaceRepository
{
    Task<IReadOnlyList<NearbyPlace>> GetByVenueAsync(
        Guid venueId,
        CancellationToken cancellationToken = default);

    Task<NearbyPlace?> GetByIdAsync(
        Guid placeId,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        NearbyPlace place,
        CancellationToken cancellationToken = default);

    void Update(NearbyPlace place);

    void Remove(NearbyPlace place);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
