using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SEVPMS.Api.Authorization;
using SEVPMS.Application.Features.Reports.DTOs;
using SEVPMS.Application.Features.Reports.Interfaces;

namespace SEVPMS.Api.Controllers;

[ApiController]
[Route("api/reports")]
public sealed class ReportsController(IReportService service) : ControllerBase
{
    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
    [HttpGet("platform")]
    public async Task<ActionResult<PlatformReportResponse>> Platform(
        [FromQuery] ReportDateRange range,
        CancellationToken cancellationToken)
        => Ok(await service.GetPlatformAsync(range, cancellationToken));

    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
    [HttpGet("platform.csv")]
    public async Task<IActionResult> PlatformCsv(
        [FromQuery] ReportDateRange range,
        CancellationToken cancellationToken)
    {
        var csv = await service.GetPlatformCsvAsync(range, cancellationToken);
        return File(
            Encoding.UTF8.GetBytes(csv),
            "text/csv",
            $"sevpms-platform-report-{DateTime.UtcNow:yyyyMMddHHmmss}.csv");
    }

    [Authorize(Policy = AuthorizationPolicies.EventOrganizerOnly)]
    [HttpGet("organizer")]
    public async Task<ActionResult<OrganizerReportResponse>> Organizer(
        CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId)) return Unauthorized();
        return Ok(await service.GetOrganizerAsync(userId, cancellationToken));
    }

    [Authorize(Policy = AuthorizationPolicies.VenueOwnerOnly)]
    [HttpGet("venue-owner")]
    public async Task<ActionResult<VenueOwnerReportResponse>> VenueOwner(
        CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId)) return Unauthorized();
        return Ok(await service.GetVenueOwnerAsync(userId, cancellationToken));
    }

    private bool TryGetUserId(out Guid userId)
        => Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out userId);
}
