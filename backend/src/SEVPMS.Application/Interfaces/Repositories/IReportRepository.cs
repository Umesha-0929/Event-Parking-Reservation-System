using SEVPMS.Application.Features.Reports.DTOs;

namespace SEVPMS.Application.Interfaces.Repositories;

public interface IReportRepository
{
    Task<PlatformReportResponse> GetPlatformAsync(
        DateTime fromUtc,
        DateTime toUtc,
        CancellationToken cancellationToken = default);

    Task<OrganizerReportResponse> GetOrganizerAsync(
        Guid organizerUserId,
        CancellationToken cancellationToken = default);

    Task<VenueOwnerReportResponse> GetVenueOwnerAsync(
        Guid ownerUserId,
        CancellationToken cancellationToken = default);
}
