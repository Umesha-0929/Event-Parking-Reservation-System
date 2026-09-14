using SEVPMS.Domain.Entities.Parking;

namespace SEVPMS.Application.Features.Parking.Interfaces;

public interface IParkingRouteRepository
{
    Task<IReadOnlyList<ParkingNode>> GetNodesByVenueAsync(
        Guid venueId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ParkingEdge>> GetEdgesByVenueAsync(
        Guid venueId,
        CancellationToken cancellationToken = default);
}