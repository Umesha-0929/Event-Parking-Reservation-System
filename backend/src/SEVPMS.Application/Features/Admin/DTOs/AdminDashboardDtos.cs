namespace SEVPMS.Application.Features.Admin.DTOs;

public sealed class AdminDashboardStatsResponse
{
    public int TotalUsers { get; set; }
    public int ActiveUsers { get; set; }
    public int SuspendedUsers { get; set; }

    public int TotalVenues { get; set; }
    public int ActiveVenues { get; set; }

    public int TotalEvents { get; set; }
    public int PublishedEvents { get; set; }

    public int PendingBookings { get; set; }
    public int ConfirmedBookings { get; set; }

    public int SuccessfulPayments { get; set; }
    public decimal SuccessfulRevenue { get; set; }

    public DateTime GeneratedAtUtc { get; set; }
}
