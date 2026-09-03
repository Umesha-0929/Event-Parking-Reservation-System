using Microsoft.EntityFrameworkCore;
using SEVPMS.Application.Features.Audit.DTOs;
using SEVPMS.Application.Interfaces.Repositories;
using SEVPMS.Domain.Entities.Admin;

namespace SEVPMS.Infrastructure.Persistence.Repositories;

public sealed class AuditLogRepository(SEVPMSDbContext dbContext) : IAuditLogRepository
{
    public async Task AddAsync(AuditLog log, CancellationToken cancellationToken = default)
        => await dbContext.Set<AuditLog>().AddAsync(log, cancellationToken);

    public async Task<IReadOnlyList<AuditLog>> QueryAsync(
        AuditLogQuery request,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.Set<AuditLog>().AsNoTracking();

        if (request.ActorUserId.HasValue)
            query = query.Where(x => x.ActorUserId == request.ActorUserId);

        if (!string.IsNullOrWhiteSpace(request.EntityType))
        {
            var type = request.EntityType.Trim();
            query = query.Where(x => x.EntityType == type);
        }

        if (request.FromUtc.HasValue)
            query = query.Where(x => x.CreatedAtUtc >= request.FromUtc.Value);

        if (request.ToUtc.HasValue)
            query = query.Where(x => x.CreatedAtUtc <= request.ToUtc.Value);

        return await query
            .OrderByDescending(x => x.CreatedAtUtc)
            .Take(Math.Clamp(request.Take, 1, 1000))
            .ToListAsync(cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        => dbContext.SaveChangesAsync(cancellationToken);
}
