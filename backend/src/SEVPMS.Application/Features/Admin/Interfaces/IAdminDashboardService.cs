using SEVPMS.Application.Features.Admin.DTOs;

namespace SEVPMS.Application.Features.Admin.Interfaces;

public interface IAdminDashboardService
{
    Task<AdminDashboardStatsResponse> GetStatsAsync(
        CancellationToken cancellationToken = default);
}
