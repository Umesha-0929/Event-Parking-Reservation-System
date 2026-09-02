using SEVPMS.Domain.Entities.VenueRentals;

namespace SEVPMS.Application.Interfaces.Repositories;

public interface IVenueRentalRepository
{
    Task<VenueRentalRequest?> GetByIdAsync(Guid rentalId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<VenueRentalRequest>> GetByOrganizerAsync(Guid organizerUserId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<VenueRentalRequest>> GetByVenueIdsAsync(IReadOnlyCollection<Guid> venueIds, CancellationToken cancellationToken = default);
    Task AddAsync(VenueRentalRequest request, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
