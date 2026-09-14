using SEVPMS.Application.Features.Parking.DTOs;

namespace SEVPMS.Application.Features.Parking.Interfaces;

public interface IParkingRouteService
{
    Task<ParkingRouteDto?> FindRouteAsync(
        Guid venueId,
        Guid startNodeId,
        Guid endNodeId,
        bool accessibleOnly,
        CancellationToken cancellationToken = default);
}