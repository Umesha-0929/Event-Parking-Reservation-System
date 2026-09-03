using SEVPMS.Application.Features.Audit.DTOs;
using SEVPMS.Domain.Entities.Admin;

namespace SEVPMS.Application.Interfaces.Repositories;

public interface IAuditLogRepository
{
    Task AddAsync(AuditLog log, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AuditLog>> QueryAsync(AuditLogQuery query, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
