namespace SEVPMS.Application.Features.Audit.DTOs;

public sealed class AuditLogQuery
{
    public Guid? ActorUserId { get; set; }
    public string? EntityType { get; set; }
    public DateTime? FromUtc { get; set; }
    public DateTime? ToUtc { get; set; }
    public int Take { get; set; } = 200;
}

public sealed class AuditLogResponse
{
    public Guid AuditLogId { get; set; }
    public Guid? ActorUserId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public string? EntityId { get; set; }
    public string? BeforeSummary { get; set; }
    public string? AfterSummary { get; set; }
    public string? CorrelationId { get; set; }
    public string? IpAddress { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}
