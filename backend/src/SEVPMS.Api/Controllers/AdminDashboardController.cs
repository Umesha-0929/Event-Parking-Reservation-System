using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SEVPMS.Api.Authorization;
using SEVPMS.Application.Features.Admin.DTOs;
using SEVPMS.Application.Features.Admin.Interfaces;

namespace SEVPMS.Api.Controllers;

[ApiController]
[Route("api/admin/dashboard")]
[Authorize(Policy = AuthorizationPolicies.AdminOnly)]
public sealed class AdminDashboardController(
    IAdminDashboardService dashboardService)
    : ControllerBase
{
    [HttpGet("stats")]
    public async Task<ActionResult<AdminDashboardStatsResponse>> GetStats(
        CancellationToken cancellationToken)
        => Ok(await dashboardService.GetStatsAsync(
            cancellationToken));
}
