using SEVPMS.Application.Features.Reports.DTOs;
using SEVPMS.Application.Features.Reports.Services;
using SEVPMS.Application.Interfaces.Repositories;
using Xunit;

namespace SEVPMS.UnitTests.Reports;

public sealed class ReportServiceTests
{
    [Fact]
    public async Task Platform_csv_contains_revenue_metrics()
    {
        var service = new ReportService(new FakeReportRepository());

        var csv = await service.GetPlatformCsvAsync(
            new ReportDateRange
            {
                FromUtc = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                ToUtc = new DateTime(2026, 2, 1, 0, 0, 0, DateTimeKind.Utc)
            });

        Assert.Contains("GrossRevenue,1000.00", csv);
        Assert.Contains("RefundedAmount,100.00", csv);
        Assert.Contains("NetRevenue,900.00", csv);
    }

    private sealed class FakeReportRepository : IReportRepository
    {
        public Task<PlatformReportResponse> GetPlatformAsync(
            DateTime fromUtc,
            DateTime toUtc,
            CancellationToken cancellationToken = default)
            => Task.FromResult(
                new PlatformReportResponse
                {
                    FromUtc = fromUtc,
                    ToUtc = toUtc,
                    GrossRevenue = 1000m,
                    RefundedAmount = 100m,
                    NetRevenue = 900m
                });

        public Task<OrganizerReportResponse> GetOrganizerAsync(
            Guid organizerUserId,
            CancellationToken cancellationToken = default)
            => Task.FromResult(
                new OrganizerReportResponse
                {
                    OrganizerUserId = organizerUserId
                });

        public Task<VenueOwnerReportResponse> GetVenueOwnerAsync(
            Guid ownerUserId,
            CancellationToken cancellationToken = default)
            => Task.FromResult(
                new VenueOwnerReportResponse
                {
                    VenueOwnerUserId = ownerUserId
                });
    }
}
