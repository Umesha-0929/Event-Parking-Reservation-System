namespace SEVPMS.Application.Features.Waitlists.DTOs;

public sealed class WaitlistEligibilityResultDto
{
    public Guid EventId { get; set; }

    public int RequestedCount { get; set; }

    public int EligibleCount { get; set; }

    public IReadOnlyList<Guid> CustomerUserIds { get; set; }
        = [];
}
