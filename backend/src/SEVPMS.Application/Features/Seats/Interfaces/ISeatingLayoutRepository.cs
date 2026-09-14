using SEVPMS.Domain.Entities.Seats;

namespace SEVPMS.Application.Features.Seats.Interfaces;

public interface ISeatingLayoutRepository
{
    Task<SeatingLayout?> GetLayoutByEventAsync(
        Guid eventId,
        CancellationToken cancellationToken = default);

    Task<SeatingLayout?> GetPublishedLayoutByEventAsync(
        Guid eventId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<SeatSection>> GetSectionsAsync(
        Guid seatingLayoutId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<SeatCategory>> GetCategoriesAsync(
        Guid seatingLayoutId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<Seat>> GetSeatsAsync(
        Guid eventId,
        CancellationToken cancellationToken = default);

    Task<SeatingLayout> AddLayoutAsync(
        SeatingLayout layout,
        CancellationToken cancellationToken = default);

    Task UpdateLayoutAsync(
        SeatingLayout layout,
        CancellationToken cancellationToken = default);

    Task<SeatSection> UpsertSectionAsync(
        SeatSection section,
        CancellationToken cancellationToken = default);

    Task<SeatCategory> UpsertCategoryAsync(
        SeatCategory category,
        CancellationToken cancellationToken = default);

    Task ReplaceSectionSeatsAsync(
        Guid eventId,
        Guid sectionId,
        IReadOnlyCollection<Seat> seats,
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(
        CancellationToken cancellationToken = default);
}
