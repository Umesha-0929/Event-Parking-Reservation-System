using SEVPMS.Application.Features.Reports.DTOs;

namespace SEVPMS.Application.Features.Reports.Interfaces;

public interface IReportService
{
    Task<PlatformReportResponse> GetPlatformAsync(
        ReportDateRange range,
        CancellationToken cancellationToken = default);

    Task<string> GetPlatformCsvAsync(
        ReportDateRange range,
        CancellationToken cancellationToken = default);

    Task<OrganizerReportResponse> GetOrganizerAsync(
        Guid organizerUserId,
        CancellationToken cancellationToken = default);

    Task<VenueOwnerReportResponse> GetVenueOwnerAsync(
        Guid ownerUserId,
        CancellationToken cancellationToken = default);
}
