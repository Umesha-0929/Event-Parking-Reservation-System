using SEVPMS.Domain.Enums;

namespace SEVPMS.Application.Features.Waitlists.DTOs;

public sealed class WaitlistEntryDto
{
    public Guid Id { get; set; }

    public Guid EventId { get; set; }

    public Guid CustomerUserId { get; set; }

    public WaitlistStatus Status { get; set; }

    public int? Position { get; set; }

    public DateTime JoinedAtUtc { get; set; }

    public DateTime? EligibleAtUtc { get; set; }
}