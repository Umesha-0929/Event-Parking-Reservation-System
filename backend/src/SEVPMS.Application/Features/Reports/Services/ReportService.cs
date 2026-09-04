using System.Globalization;
using SEVPMS.Application.Features.Reports.DTOs;
using SEVPMS.Application.Features.Reports.Interfaces;
using SEVPMS.Application.Interfaces.Repositories;

namespace SEVPMS.Application.Features.Reports.Services;

public sealed class ReportService(IReportRepository repository) : IReportService
{
    public Task<PlatformReportResponse> GetPlatformAsync(
        ReportDateRange range,
        CancellationToken cancellationToken = default)
    {
        var (from, to) = Normalize(range);
        return repository.GetPlatformAsync(from, to, cancellationToken);
    }

    public async Task<string> GetPlatformCsvAsync(
        ReportDateRange range,
        CancellationToken cancellationToken = default)
    {
        var report = await GetPlatformAsync(range, cancellationToken);

        var lines = new[]
        {
            "Metric,Value",
            $"FromUtc,{report.FromUtc:O}",
            $"ToUtc,{report.ToUtc:O}",
            $"Users,{report.Users}",
            $"Events,{report.Events}",
            $"PublishedEvents,{report.PublishedEvents}",
            $"Venues,{report.Venues}",
            $"Bookings,{report.Bookings}",
            $"ConfirmedBookings,{report.ConfirmedBookings}",
            $"SuccessfulPayments,{report.SuccessfulPayments}",
            $"Refunds,{report.Refunds}",
            $"Attendance,{report.Attendance}",
            $"ParkingReservations,{report.ParkingReservations}",
            $"FoodOrders,{report.FoodOrders}",
            $"GrossRevenue,{report.GrossRevenue.ToString("0.00", CultureInfo.InvariantCulture)}",
            $"RefundedAmount,{report.RefundedAmount.ToString("0.00", CultureInfo.InvariantCulture)}",
            $"NetRevenue,{report.NetRevenue.ToString("0.00", CultureInfo.InvariantCulture)}",
            $"FoodRevenue,{report.FoodRevenue.ToString("0.00", CultureInfo.InvariantCulture)}"

        };

        return string.Join(Environment.NewLine, lines);
    }

    public Task<OrganizerReportResponse> GetOrganizerAsync(
        Guid organizerUserId,
        CancellationToken cancellationToken = default)
        => repository.GetOrganizerAsync(organizerUserId, cancellationToken);

    public Task<VenueOwnerReportResponse> GetVenueOwnerAsync(
        Guid ownerUserId,
        CancellationToken cancellationToken = default)
        => repository.GetVenueOwnerAsync(ownerUserId, cancellationToken);

    private static (DateTime from, DateTime to) Normalize(ReportDateRange range)
    {
        var to = range.ToUtc ?? DateTime.UtcNow.AddDays(1);
        var from = range.FromUtc ?? to.AddDays(-30);

        if (to <= from)
            throw new ArgumentException("Report end date must be later than start date.");

        return (from, to);
    }
}
