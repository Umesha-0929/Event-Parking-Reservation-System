using SEVPMS.Domain.Common;

namespace SEVPMS.Domain.Entities.Notifications;

public sealed class Notification : AuditableEntity
{
    public Guid UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Type { get; set; } = "General";
    public bool IsRead { get; set; }
    public DateTime? ReadAtUtc { get; set; }
}
