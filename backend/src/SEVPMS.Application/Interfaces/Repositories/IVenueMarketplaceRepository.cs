using SEVPMS.Domain.Entities.Venues;

namespace SEVPMS.Application.Interfaces.Repositories;

public interface IVenueMarketplaceRepository
{
    Task<IReadOnlyList<VenueFacility>> GetFacilitiesAsync(bool includeInactive, CancellationToken cancellationToken = default);
    Task<VenueFacility?> GetFacilityAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddFacilityAsync(VenueFacility facility, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<VenueFacilityLink>> GetFacilityLinksAsync(Guid venueId, CancellationToken cancellationToken = default);
    Task ReplaceFacilityLinksAsync(Guid venueId, IReadOnlyCollection<Guid> facilityIds, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<VenueMedia>> GetMediaAsync(Guid venueId, CancellationToken cancellationToken = default);
    Task AddMediaAsync(VenueMedia media, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<VenueRate>> GetRatesAsync(Guid venueId, CancellationToken cancellationToken = default);
    Task AddRateAsync(VenueRate rate, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<VenueAvailability>> GetAvailabilityAsync(Guid venueId, CancellationToken cancellationToken = default);
    Task<bool> HasBlockingAvailabilityAsync(Guid venueId, DateTime startAtUtc, DateTime endAtUtc, CancellationToken cancellationToken = default);
    Task AddAvailabilityAsync(VenueAvailability availability, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<VenueLayoutTemplate>> GetLayoutTemplatesAsync(Guid venueId, CancellationToken cancellationToken = default);
    Task AddLayoutTemplateAsync(VenueLayoutTemplate template, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
