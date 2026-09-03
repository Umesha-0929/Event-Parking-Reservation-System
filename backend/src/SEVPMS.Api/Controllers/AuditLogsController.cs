using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SEVPMS.Api.Authorization;
using SEVPMS.Application.Features.Audit.DTOs;
using SEVPMS.Application.Features.Audit.Interfaces;

namespace SEVPMS.Api.Controllers;

[ApiController]
[Route("api/admin/audit-logs")]
[Authorize(Policy = AuthorizationPolicies.AdminOnly)]
public sealed class AuditLogsController(IAuditLogService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<AuditLogResponse>>> Get(
        [FromQuery] AuditLogQuery query,
        CancellationToken cancellationToken)
        => Ok(await service.QueryAsync(query, cancellationToken));
}
