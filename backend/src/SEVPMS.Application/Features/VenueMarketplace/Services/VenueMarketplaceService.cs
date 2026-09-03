using SEVPMS.Application.Common.Exceptions;
using SEVPMS.Application.Features.VenueMarketplace.DTOs;
using SEVPMS.Application.Features.VenueMarketplace.Interfaces;
using SEVPMS.Application.Interfaces.Repositories;
using SEVPMS.Domain.Entities.Venues;

namespace SEVPMS.Application.Features.VenueMarketplace.Services;

public sealed class VenueMarketplaceService(
    IVenueMarketplaceRepository marketplaceRepository,
    IVenueRepository venueRepository)
    : IVenueMarketplaceService
{
    public async Task<IReadOnlyList<FacilityResponse>> GetFacilitiesAsync(
        bool includeInactive,
        CancellationToken cancellationToken = default)
        => (await marketplaceRepository.GetFacilitiesAsync(includeInactive, cancellationToken))
            .Select(Map)
            .ToList();

    public async Task<FacilityResponse> CreateFacilityAsync(
        UpsertFacilityRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidateFacility(request);
        var entity = new VenueFacility
        {
            Name = request.Name.Trim(),
            Category = request.Category.Trim(),
            IsActive = request.IsActive
        };

        await marketplaceRepository.AddFacilityAsync(entity, cancellationToken);
        await marketplaceRepository.SaveChangesAsync(cancellationToken);
        return Map(entity);
    }

    public async Task<FacilityResponse> UpdateFacilityAsync(
        Guid id,
        UpsertFacilityRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidateFacility(request);
        var entity = await marketplaceRepository.GetFacilityAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException("Venue facility was not found.");

        entity.Name = request.Name.Trim();
        entity.Category = request.Category.Trim();
        entity.IsActive = request.IsActive;
        entity.UpdatedAtUtc = DateTime.UtcNow;

        await marketplaceRepository.SaveChangesAsync(cancellationToken);
        return Map(entity);
    }

    public async Task<VenueMarketplaceResponse> GetVenueAsync(
        Guid venueId,
        CancellationToken cancellationToken = default)
    {
        var venue = await venueRepository.GetByIdAsync(venueId, cancellationToken)
            ?? throw new KeyNotFoundException("Venue was not found.");

        if (!venue.IsActive)
            throw new KeyNotFoundException("Venue was not found.");

        var facilities = await marketplaceRepository.GetFacilitiesAsync(false, cancellationToken);
        var links = await marketplaceRepository.GetFacilityLinksAsync(venueId, cancellationToken);
        var linkedIds = links.Select(x => x.FacilityId).ToHashSet();

        var media = await marketplaceRepository.GetMediaAsync(venueId, cancellationToken);
        var rates = await marketplaceRepository.GetRatesAsync(venueId, cancellationToken);
        var availability = await marketplaceRepository.GetAvailabilityAsync(venueId, cancellationToken);
        var layouts = await marketplaceRepository.GetLayoutTemplatesAsync(venueId, cancellationToken);

        return new VenueMarketplaceResponse
        {
            VenueId = venueId,
            Facilities = facilities.Where(x => linkedIds.Contains(x.Id)).Select(Map).ToList(),
            Media = media.Select(x => new VenueMediaResponse
            {
                VenueMediaId = x.Id,
                Url = x.Url,
                Type = x.Type,
                SortOrder = x.SortOrder
            }).ToList(),
            Rates = rates.Select(x => new VenueRateResponse
            {
                VenueRateId = x.Id,
                RateType = x.RateType,
                Amount = x.Amount,
                Currency = x.Currency,
                ValidFromUtc = x.ValidFromUtc,
                ValidToUtc = x.ValidToUtc
            }).ToList(),
            Availability = availability.Select(x => new VenueAvailabilityResponse
            {
                VenueAvailabilityId = x.Id,
                StartAtUtc = x.StartAtUtc,
                EndAtUtc = x.EndAtUtc,
                Type = x.Type,
                Notes = x.Notes
            }).ToList(),
            LayoutTemplates = layouts.Select(x => new VenueLayoutTemplateResponse
            {
                VenueLayoutTemplateId = x.Id,
                Name = x.Name,
                Version = x.Version,
                LayoutJson = x.LayoutJson,
                IsActive = x.IsActive
            }).ToList()
        };
    }

    public async Task SetFacilitiesAsync(
        Guid ownerUserId,
        Guid venueId,
        SetVenueFacilitiesRequest request,
        CancellationToken cancellationToken = default)
    {
        await EnsureOwnerAsync(ownerUserId, venueId, cancellationToken);

        var activeFacilities = await marketplaceRepository.GetFacilitiesAsync(false, cancellationToken);
        var activeIds = activeFacilities.Select(x => x.Id).ToHashSet();
        var requested = request.FacilityIds?.Distinct().ToArray() ?? Array.Empty<Guid>();

        if (requested.Any(x => !activeIds.Contains(x)))
            throw new ArgumentException("One or more venue facilities are invalid.");

        await marketplaceRepository.ReplaceFacilityLinksAsync(venueId, requested, cancellationToken);
        await marketplaceRepository.SaveChangesAsync(cancellationToken);
    }

    public async Task<VenueMediaResponse> AddMediaAsync(
        Guid ownerUserId,
        Guid venueId,
        AddVenueMediaRequest request,
        CancellationToken cancellationToken = default)
    {
        await EnsureOwnerAsync(ownerUserId, venueId, cancellationToken);

        if (string.IsNullOrWhiteSpace(request.Url))
            throw new ArgumentException("Media URL is required.");

        var entity = new VenueMedia
        {
            VenueId = venueId,
            Url = request.Url.Trim(),
            Type = string.IsNullOrWhiteSpace(request.Type) ? "Photo" : request.Type.Trim(),
            SortOrder = request.SortOrder
        };

        await marketplaceRepository.AddMediaAsync(entity, cancellationToken);
        await marketplaceRepository.SaveChangesAsync(cancellationToken);

        return new VenueMediaResponse
        {
            VenueMediaId = entity.Id,
            Url = entity.Url,
            Type = entity.Type,
            SortOrder = entity.SortOrder
        };
    }

    public async Task<VenueRateResponse> AddRateAsync(
        Guid ownerUserId,
        Guid venueId,
        AddVenueRateRequest request,
        CancellationToken cancellationToken = default)
    {
        await EnsureOwnerAsync(ownerUserId, venueId, cancellationToken);

        if (request.Amount < 0)
            throw new ArgumentException("Venue rate cannot be negative.");

        if (request.ValidFromUtc.HasValue &&
            request.ValidToUtc.HasValue &&
            request.ValidToUtc <= request.ValidFromUtc)
        {
            throw new ArgumentException("Venue rate end date must be later than start date.");
        }

        var entity = new VenueRate
        {
            VenueId = venueId,
            RateType = string.IsNullOrWhiteSpace(request.RateType) ? "Hourly" : request.RateType.Trim(),
            Amount = request.Amount,
            Currency = string.IsNullOrWhiteSpace(request.Currency) ? "LKR" : request.Currency.Trim().ToUpperInvariant(),
            ValidFromUtc = request.ValidFromUtc,
            ValidToUtc = request.ValidToUtc,
            IsActive = true
        };

        await marketplaceRepository.AddRateAsync(entity, cancellationToken);
        await marketplaceRepository.SaveChangesAsync(cancellationToken);

        return new VenueRateResponse
        {
            VenueRateId = entity.Id,
            RateType = entity.RateType,
            Amount = entity.Amount,
            Currency = entity.Currency,
            ValidFromUtc = entity.ValidFromUtc,
            ValidToUtc = entity.ValidToUtc
        };
    }

    public async Task<VenueAvailabilityResponse> AddAvailabilityAsync(
        Guid ownerUserId,
        Guid venueId,
        AddVenueAvailabilityRequest request,
        CancellationToken cancellationToken = default)
    {
        await EnsureOwnerAsync(ownerUserId, venueId, cancellationToken);

        if (request.EndAtUtc <= request.StartAtUtc)
            throw new ArgumentException("Availability end time must be later than start time.");

        var entity = new VenueAvailability
        {
            VenueId = venueId,
            StartAtUtc = request.StartAtUtc,
            EndAtUtc = request.EndAtUtc,
            Type = request.Type,
            Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim()
        };

        await marketplaceRepository.AddAvailabilityAsync(entity, cancellationToken);
        await marketplaceRepository.SaveChangesAsync(cancellationToken);

        return new VenueAvailabilityResponse
        {
            VenueAvailabilityId = entity.Id,
            StartAtUtc = entity.StartAtUtc,
            EndAtUtc = entity.EndAtUtc,
            Type = entity.Type,
            Notes = entity.Notes
        };
    }

    public async Task<VenueLayoutTemplateResponse> AddLayoutTemplateAsync(
        Guid ownerUserId,
        Guid venueId,
        AddVenueLayoutTemplateRequest request,
        CancellationToken cancellationToken = default)
    {
        await EnsureOwnerAsync(ownerUserId, venueId, cancellationToken);

        if (string.IsNullOrWhiteSpace(request.Name))
            throw new ArgumentException("Layout template name is required.");
        if (request.Version <= 0)
            throw new ArgumentException("Layout template version must be positive.");
        if (string.IsNullOrWhiteSpace(request.LayoutJson))
            throw new ArgumentException("Layout metadata is required.");

        var entity = new VenueLayoutTemplate
        {
            VenueId = venueId,
            Name = request.Name.Trim(),
            Version = request.Version,
            LayoutJson = request.LayoutJson.Trim(),
            IsActive = true
        };

        await marketplaceRepository.AddLayoutTemplateAsync(entity, cancellationToken);
        await marketplaceRepository.SaveChangesAsync(cancellationToken);

        return new VenueLayoutTemplateResponse
        {
            VenueLayoutTemplateId = entity.Id,
            Name = entity.Name,
            Version = entity.Version,
            LayoutJson = entity.LayoutJson,
            IsActive = entity.IsActive
        };
    }

    private async Task EnsureOwnerAsync(
        Guid ownerUserId,
        Guid venueId,
        CancellationToken cancellationToken)
    {
        var venue = await venueRepository.GetByIdAsync(venueId, cancellationToken)
            ?? throw new KeyNotFoundException("Venue was not found.");

        if (venue.OwnerUserId != ownerUserId)
            throw new ForbiddenAccessException("You do not own this venue.");
    }

    private static void ValidateFacility(UpsertFacilityRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            throw new ArgumentException("Facility name is required.");
        if (string.IsNullOrWhiteSpace(request.Category))
            throw new ArgumentException("Facility category is required.");
    }

    private static FacilityResponse Map(VenueFacility x) => new()
    {
        FacilityId = x.Id,
        Name = x.Name,
        Category = x.Category,
        IsActive = x.IsActive
    };
}
