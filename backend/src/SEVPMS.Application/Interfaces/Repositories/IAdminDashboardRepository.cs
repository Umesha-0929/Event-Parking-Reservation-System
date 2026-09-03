using SEVPMS.Application.Features.Admin.DTOs;

namespace SEVPMS.Application.Interfaces.Repositories;

public interface IAdminDashboardRepository
{
    Task<AdminDashboardStatsResponse> GetStatsAsync(
        CancellationToken cancellationToken = default);
}
