using SEVPMS.Application.Features.Places.DTOs;
using SEVPMS.Application.Features.Places.Interfaces;
using SEVPMS.Application.Features.Places.Validators;
using SEVPMS.Domain.Entities.Places;

namespace SEVPMS.Application.Features.Places.Services;

public sealed class PlaceFinderService(IPlaceRepository placeRepository)
    : IPlaceFinderService
{
    private static readonly string[] SupportedAudienceModes =
    [
        "Individual",
        "Couple",
        "Friends",
        "FamilyChildren"
    ];

    public async Task<IReadOnlyList<NearbyPlaceDto>> BrowseAsync(
        Guid venueId,
        PlaceFinderRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidateVenue(venueId);
        request ??= new PlaceFinderRequest();

        var places = await placeRepository.GetByVenueAsync(venueId, cancellationToken);
        return ApplyCommonFilters(places, request)
            .OrderBy(place => place.DistanceKm)
            .ThenBy(place => place.Name)
            .Select(place => Map(place, string.Empty))
            .ToList();
    }

    public async Task<IReadOnlyList<NearbyPlaceDto>> RecommendAsync(
        Guid venueId,
        PlaceFinderRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidateVenue(venueId);
        ArgumentNullException.ThrowIfNull(request);

        var audienceMode = NormalizeAudienceMode(request.AudienceMode);
        var places = await placeRepository.GetByVenueAsync(venueId, cancellationToken);

        return ApplyCommonFilters(places, request)
            .Where(place => GetCsvValues(place.AudienceModesCsv)
                .Contains(audienceMode, StringComparer.OrdinalIgnoreCase))
            .OrderByDescending(place => place.IsOpen)
            .ThenBy(place => place.DistanceKm)
            .ThenBy(place => place.Name)
            .Select(place => Map(
                place,
                $"Recommended for {DisplayAudienceMode(audienceMode)}; {place.DistanceKm:0.##} km from the venue."))
            .ToList();
    }

    public async Task<NearbyPlaceDto> CreateAsync(
        ManageNearbyPlaceRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidateManageRequest(request);

        var place = new NearbyPlace();
        Apply(place, request);

        await placeRepository.AddAsync(place, cancellationToken);
        await placeRepository.SaveChangesAsync(cancellationToken);
        return Map(place, string.Empty);
    }

    public async Task<NearbyPlaceDto> UpdateAsync(
        Guid placeId,
        ManageNearbyPlaceRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidateManageRequest(request);

        var place = await placeRepository.GetByIdAsync(placeId, cancellationToken)
            ?? throw new KeyNotFoundException("Nearby place was not found.");

        Apply(place, request);
        place.UpdatedAtUtc = DateTime.UtcNow;
        placeRepository.Update(place);
        await placeRepository.SaveChangesAsync(cancellationToken);
        return Map(place, string.Empty);
    }

    public async Task DeleteAsync(
        Guid placeId,
        CancellationToken cancellationToken = default)
    {
        var place = await placeRepository.GetByIdAsync(placeId, cancellationToken)
            ?? throw new KeyNotFoundException("Nearby place was not found.");

        placeRepository.Remove(place);
        await placeRepository.SaveChangesAsync(cancellationToken);
    }

    private static IEnumerable<NearbyPlace> ApplyCommonFilters(
        IEnumerable<NearbyPlace> places,
        PlaceFinderRequest request)
    {
        var query = places.Where(place => place.IsActive);

        if (!request.IncludeClosed)
        {
            query = query.Where(place => place.IsOpen);
        }

        if (!string.IsNullOrWhiteSpace(request.Category))
        {
            query = query.Where(place =>
                place.Category.Equals(request.Category.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        if (request.MaxDistanceKm.HasValue)
        {
            if (request.MaxDistanceKm.Value < 0)
            {
                throw new PlaceFinderValidationException(
                    "Maximum distance cannot be negative.");
            }

            query = query.Where(place => place.DistanceKm <= request.MaxDistanceKm.Value);
        }

        return query;
    }

    private static void ValidateManageRequest(ManageNearbyPlaceRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ValidateVenue(request.VenueId);

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            throw new PlaceFinderValidationException("Place name is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Category))
        {
            throw new PlaceFinderValidationException("Place category is required.");
        }

        if (request.DistanceKm < 0)
        {
            throw new PlaceFinderValidationException("Distance cannot be negative.");
        }

        var normalizedModes = request.AudienceModes
            .Where(mode => !string.IsNullOrWhiteSpace(mode))
            .Select(NormalizeAudienceMode)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (normalizedModes.Count == 0)
        {
            throw new PlaceFinderValidationException(
                "At least one audience mode is required.");
        }
    }

    private static void ValidateVenue(Guid venueId)
    {
        if (venueId == Guid.Empty)
        {
            throw new PlaceFinderValidationException("Venue is required.");
        }
    }

    private static string NormalizeAudienceMode(string? audienceMode)
    {
        var value = audienceMode?.Trim().Replace("/", string.Empty).Replace(" ", string.Empty);

        var normalized = value?.ToLowerInvariant() switch
        {
            "individual" => "Individual",
            "single" => "Individual",
            "couple" => "Couple",
            "friends" => "Friends",
            "familychildren" => "FamilyChildren",
            "family" => "FamilyChildren",
            "children" => "FamilyChildren",
            _ => null
        };

        if (normalized is null ||
            !SupportedAudienceModes.Contains(normalized, StringComparer.OrdinalIgnoreCase))
        {
            throw new PlaceFinderValidationException(
                "Audience mode must be Individual, Couple, Friends, or Family/Children.");
        }

        return normalized;
    }

    private static string DisplayAudienceMode(string audienceMode)
        => audienceMode == "FamilyChildren" ? "Family/Children" : audienceMode;

    private static void Apply(NearbyPlace place, ManageNearbyPlaceRequest request)
    {
        var modes = request.AudienceModes
            .Where(mode => !string.IsNullOrWhiteSpace(mode))
            .Select(NormalizeAudienceMode)
            .Distinct(StringComparer.OrdinalIgnoreCase);

        var tags = request.Tags
            .Where(tag => !string.IsNullOrWhiteSpace(tag))
            .Select(tag => tag.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase);

        place.VenueId = request.VenueId;
        place.Name = request.Name.Trim();
        place.Category = request.Category.Trim();
        place.TagsCsv = string.Join(',', tags);
        place.AudienceModesCsv = string.Join(',', modes);
        place.Address = request.Address?.Trim() ?? string.Empty;
        place.DistanceKm = request.DistanceKm;
        place.Latitude = request.Latitude;
        place.Longitude = request.Longitude;
        place.IsOpen = request.IsOpen;
        place.DirectionsUrl = string.IsNullOrWhiteSpace(request.DirectionsUrl)
            ? null
            : request.DirectionsUrl.Trim();
        place.IsActive = request.IsActive;
    }

    private static NearbyPlaceDto Map(NearbyPlace place, string reason)
        => new()
        {
            Id = place.Id,
            VenueId = place.VenueId,
            Name = place.Name,
            Category = place.Category,
            Tags = GetCsvValues(place.TagsCsv),
            AudienceModes = GetCsvValues(place.AudienceModesCsv)
                .Select(DisplayAudienceMode)
                .ToList(),
            Address = place.Address,
            DistanceKm = place.DistanceKm,
            Latitude = place.Latitude,
            Longitude = place.Longitude,
            IsOpen = place.IsOpen,
            DirectionsUrl = place.DirectionsUrl,
            RecommendationReason = reason
        };

    private static IReadOnlyList<string> GetCsvValues(string? value)
        => (value ?? string.Empty)
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToList();
}
