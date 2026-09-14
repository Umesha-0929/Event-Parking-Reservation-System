using SEVPMS.Application.Features.Seats.DTOs;

namespace SEVPMS.Application.Features.Seats.Interfaces;

public interface ISeatingLayoutService
{
    Task<SeatingLayoutDto?> GetOrganizerLayoutAsync(
        Guid eventId,
        CancellationToken cancellationToken = default);

    Task<SeatingLayoutDto> ConfigureLayoutAsync(
        Guid eventId,
        Guid organizerUserId,
        ConfigureSeatingLayoutRequest request,
        CancellationToken cancellationToken = default);

    Task<SeatSectionDto> UpsertSectionAsync(
        Guid eventId,
        Guid organizerUserId,
        UpsertSeatSectionRequest request,
        CancellationToken cancellationToken = default);

    Task<SeatCategoryDto> UpsertCategoryAsync(
        Guid eventId,
        Guid organizerUserId,
        UpsertSeatCategoryRequest request,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<SeatAvailabilityDto>> GenerateSeatsAsync(
        Guid eventId,
        Guid organizerUserId,
        GenerateSeatsRequest request,
        CancellationToken cancellationToken = default);

    Task<SeatingLayoutDto> PublishLayoutAsync(
        Guid eventId,
        Guid organizerUserId,
        PublishSeatingLayoutRequest request,
        CancellationToken cancellationToken = default);

    Task<PublishedSeatingLayoutDto?> GetPublishedLayoutAsync(
        Guid eventId,
        CancellationToken cancellationToken = default);
}
