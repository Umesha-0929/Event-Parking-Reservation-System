using System.Security.Claims;
using SEVPMS.Domain.Enums;

namespace SEVPMS.Api.Klegar;

public sealed class RequestUserResolver(
    IConfiguration configuration,
    IWebHostEnvironment environment)
{
    public bool TryGetUserId(
        HttpContext context,
        out Guid userId)
    {
        userId = Guid.Empty;

        var raw =
            context.User.FindFirstValue(
                ClaimTypes.NameIdentifier)
            ?? context.User.FindFirstValue("sub");

        if (Guid.TryParse(raw, out userId))
        {
            return true;
        }

        if (environment.IsDevelopment() &&
            configuration.GetValue(
                "Klegar:AllowDevelopmentUserHeader",
                true) &&
            context.Request.Headers.TryGetValue(
                "X-SEVPMS-Demo-UserId",
                out var values) &&
            Guid.TryParse(
                values.FirstOrDefault(),
                out userId))
        {
            return true;
        }

        return false;
    }

    public bool IsOrganizerOrAdmin(
        HttpContext context)
    {
        if (context.User.IsInRole(
                UserRole.EventOrganizer.ToString()) ||
            context.User.IsInRole(
                UserRole.Admin.ToString()))
        {
            return true;
        }

        if (!environment.IsDevelopment() ||
            !configuration.GetValue(
                "Klegar:AllowDevelopmentUserHeader",
                true) ||
            !context.Request.Headers.TryGetValue(
                "X-SEVPMS-Demo-Role",
                out var values))
        {
            return false;
        }

        var role = values.FirstOrDefault();

        return string.Equals(
                   role,
                   UserRole.EventOrganizer.ToString(),
                   StringComparison.OrdinalIgnoreCase) ||
               string.Equals(
                   role,
                   "Organizer",
                   StringComparison.OrdinalIgnoreCase) ||
               string.Equals(
                   role,
                   UserRole.Admin.ToString(),
                   StringComparison.OrdinalIgnoreCase);
    }
}