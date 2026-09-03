using SEVPMS.Application.Features.VenueMarketplace.DTOs;

namespace SEVPMS.Application.Features.VenueMarketplace.Interfaces;

public interface IVenueMarketplaceService
{
    Task<IReadOnlyList<FacilityResponse>> GetFacilitiesAsync(bool includeInactive, CancellationToken cancellationToken = default);
    Task<FacilityResponse> CreateFacilityAsync(UpsertFacilityRequest request, CancellationToken cancellationToken = default);
    Task<FacilityResponse> UpdateFacilityAsync(Guid id, UpsertFacilityRequest request, CancellationToken cancellationToken = default);
    Task<VenueMarketplaceResponse> GetVenueAsync(Guid venueId, CancellationToken cancellationToken = default);
    Task SetFacilitiesAsync(Guid ownerUserId, Guid venueId, SetVenueFacilitiesRequest request, CancellationToken cancellationToken = default);
    Task<VenueMediaResponse> AddMediaAsync(Guid ownerUserId, Guid venueId, AddVenueMediaRequest request, CancellationToken cancellationToken = default);
    Task<VenueRateResponse> AddRateAsync(Guid ownerUserId, Guid venueId, AddVenueRateRequest request, CancellationToken cancellationToken = default);
    Task<VenueAvailabilityResponse> AddAvailabilityAsync(Guid ownerUserId, Guid venueId, AddVenueAvailabilityRequest request, CancellationToken cancellationToken = default);
    Task<VenueLayoutTemplateResponse> AddLayoutTemplateAsync(Guid ownerUserId, Guid venueId, AddVenueLayoutTemplateRequest request, CancellationToken cancellationToken = default);
}
