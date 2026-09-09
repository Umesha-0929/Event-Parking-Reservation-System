using System.Security.Claims;
using SEVPMS.Domain.Enums;

namespace SEVPMS.Api.Klegar;

public sealed class RequestUserResolver
{
    public bool TryGetUserId(HttpContext context, out Guid userId)
    {
        userId = Guid.Empty;
        var raw = context.User.FindFirstValue(ClaimTypes.NameIdentifier)
                  ?? context.User.FindFirstValue("sub");
        return Guid.TryParse(raw, out userId);
    }

    public bool IsOrganizerOrAdmin(HttpContext context)
        => context.User.IsInRole(UserRole.EventOrganizer.ToString())
           || context.User.IsInRole(UserRole.Admin.ToString());
}
