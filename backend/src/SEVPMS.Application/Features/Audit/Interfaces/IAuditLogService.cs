using SEVPMS.Application.Features.Audit.DTOs;

namespace SEVPMS.Application.Features.Audit.Interfaces;

public interface IAuditLogService
{
    Task WriteAsync(
        Guid? actorUserId,
        string action,
        string entityType,
        string? entityId,
        string? beforeSummary,
        string? afterSummary,
        string? correlationId,
        string? ipAddress,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AuditLogResponse>> QueryAsync(
        AuditLogQuery query,
        CancellationToken cancellationToken = default);
}
