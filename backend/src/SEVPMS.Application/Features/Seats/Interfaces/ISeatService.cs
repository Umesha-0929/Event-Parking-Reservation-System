using SEVPMS.Application.Features.Seats.DTOs;
namespace SEVPMS.Application.Features.Seats.Interfaces;
public interface ISeatService
{
    Task<IReadOnlyList<SeatAvailabilityDto>> GetAvailabilityAsync(Guid eventId, Guid? sectionId, CancellationToken cancellationToken = default);
    Task<SeatHoldResponse> HoldAsync(Guid eventId, Guid userId, CreateSeatHoldRequest request, CancellationToken cancellationToken = default);
    Task<bool> ReleaseHoldAsync(string holdToken, Guid userId, CancellationToken cancellationToken = default);
    Task<bool> CommitHoldAsync(string holdToken, Guid userId, Guid bookingId, CancellationToken cancellationToken = default);
    Task<SeatAvailabilityDto> UpsertSeatAsync(Guid eventId, UpsertSeatRequest request, CancellationToken cancellationToken = default);
    Task<SeatViewAssetDto?> GetSeatViewAsync(Guid eventId, Guid seatId, CancellationToken cancellationToken = default);
    Task<SeatViewAssetDto> UpsertSeatViewAsync(Guid eventId, UpsertSeatViewAssetRequest request, CancellationToken cancellationToken = default);
}
