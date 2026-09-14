using SEVPMS.Domain.Enums;

namespace SEVPMS.Application.Features.VenueMarketplace.DTOs;

public sealed class FacilityResponse
{
    public Guid FacilityId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

public sealed class UpsertFacilityRequest
{
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}

public sealed class SetVenueFacilitiesRequest
{
    public IReadOnlyCollection<Guid> FacilityIds { get; set; } = Array.Empty<Guid>();
}

public sealed class AddVenueMediaRequest
{
    public string Url { get; set; } = string.Empty;
    public string Type { get; set; } = "Photo";
    public int SortOrder { get; set; }
}

public sealed class AddVenueRateRequest
{
    public string RateType { get; set; } = "Hourly";
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "LKR";
    public DateTime? ValidFromUtc { get; set; }
    public DateTime? ValidToUtc { get; set; }
}

public sealed class AddVenueAvailabilityRequest
{
    public DateTime StartAtUtc { get; set; }
    public DateTime EndAtUtc { get; set; }
    public VenueAvailabilityType Type { get; set; }
    public string? Notes { get; set; }
}

public sealed class AddVenueLayoutTemplateRequest
{
    public string Name { get; set; } = string.Empty;
    public int Version { get; set; } = 1;
    public string LayoutJson { get; set; } = "{}";
}

public sealed class VenueMediaResponse
{
    public Guid VenueMediaId { get; set; }
    public string Url { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public int SortOrder { get; set; }
}

public sealed class VenueRateResponse
{
    public Guid VenueRateId { get; set; }
    public string RateType { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public DateTime? ValidFromUtc { get; set; }
    public DateTime? ValidToUtc { get; set; }
}

public sealed class VenueAvailabilityResponse
{
    public Guid VenueAvailabilityId { get; set; }
    public DateTime StartAtUtc { get; set; }
    public DateTime EndAtUtc { get; set; }
    public VenueAvailabilityType Type { get; set; }
    public string? Notes { get; set; }
}

public sealed class VenueLayoutTemplateResponse
{
    public Guid VenueLayoutTemplateId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Version { get; set; }
    public string LayoutJson { get; set; } = "{}";
    public bool IsActive { get; set; }
}

public sealed class VenueMarketplaceResponse
{
    public Guid VenueId { get; set; }
    public IReadOnlyList<FacilityResponse> Facilities { get; set; } = Array.Empty<FacilityResponse>();
    public IReadOnlyList<VenueMediaResponse> Media { get; set; } = Array.Empty<VenueMediaResponse>();
    public IReadOnlyList<VenueRateResponse> Rates { get; set; } = Array.Empty<VenueRateResponse>();
    public IReadOnlyList<VenueAvailabilityResponse> Availability { get; set; } = Array.Empty<VenueAvailabilityResponse>();
    public IReadOnlyList<VenueLayoutTemplateResponse> LayoutTemplates { get; set; } = Array.Empty<VenueLayoutTemplateResponse>();
}
