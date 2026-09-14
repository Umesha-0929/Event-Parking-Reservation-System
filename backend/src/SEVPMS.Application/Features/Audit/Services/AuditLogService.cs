using SEVPMS.Application.Features.Audit.DTOs;
using SEVPMS.Application.Features.Audit.Interfaces;
using SEVPMS.Application.Interfaces.Repositories;
using SEVPMS.Domain.Entities.Admin;

namespace SEVPMS.Application.Features.Audit.Services;

public sealed class AuditLogService(IAuditLogRepository repository) : IAuditLogService
{
    public async Task WriteAsync(
        Guid? actorUserId,
        string action,
        string entityType,
        string? entityId,
        string? beforeSummary,
        string? afterSummary,
        string? correlationId,
        string? ipAddress,
        CancellationToken cancellationToken = default)
    {
        await repository.AddAsync(
            new AuditLog
            {
                ActorUserId = actorUserId,
                Action = action.Trim(),
                EntityType = entityType.Trim(),
                EntityId = entityId,
                BeforeSummary = beforeSummary,
                AfterSummary = afterSummary,
                CorrelationId = correlationId,
                IpAddress = ipAddress,
                CreatedAtUtc = DateTime.UtcNow
            },
            cancellationToken);

        await repository.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<AuditLogResponse>> QueryAsync(
        AuditLogQuery query,
        CancellationToken cancellationToken = default)
        => (await repository.QueryAsync(query, cancellationToken))
            .Select(x => new AuditLogResponse
            {
                AuditLogId = x.Id,
                ActorUserId = x.ActorUserId,
                Action = x.Action,
                EntityType = x.EntityType,
                EntityId = x.EntityId,
                BeforeSummary = x.BeforeSummary,
                AfterSummary = x.AfterSummary,
                CorrelationId = x.CorrelationId,
                IpAddress = x.IpAddress,
                CreatedAtUtc = x.CreatedAtUtc
            })
            .ToList();
}
