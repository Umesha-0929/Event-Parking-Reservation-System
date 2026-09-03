using SEVPMS.Application.Features.Admin.DTOs;
using SEVPMS.Application.Features.Admin.Interfaces;
using SEVPMS.Application.Interfaces.Repositories;

namespace SEVPMS.Application.Features.Admin.Services;

public sealed class AdminDashboardService(
    IAdminDashboardRepository repository)
    : IAdminDashboardService
{
    public Task<AdminDashboardStatsResponse> GetStatsAsync(
        CancellationToken cancellationToken = default)
        => repository.GetStatsAsync(cancellationToken);
}
