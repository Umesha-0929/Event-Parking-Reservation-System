using System.Security.Claims;
namespace SEVPMS.Api.Klegar;
public sealed class RequestUserResolver(IConfiguration configuration, IWebHostEnvironment environment)
{
    public bool TryGetUserId(HttpContext context, out Guid userId)
    {
        userId = Guid.Empty;
        var raw = context.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? context.User.FindFirstValue("sub");
        if (Guid.TryParse(raw, out userId)) return true;
        if (environment.IsDevelopment() && configuration.GetValue("Klegar:AllowDevelopmentUserHeader", true) && context.Request.Headers.TryGetValue("X-SEVPMS-Demo-UserId", out var values) && Guid.TryParse(values.FirstOrDefault(), out userId)) return true;
        return false;
    }
    public bool IsOrganizerOrAdmin(HttpContext context)
    {
        if (context.User.IsInRole("Organizer") || context.User.IsInRole("Admin")) return true;
        return environment.IsDevelopment() && configuration.GetValue("Klegar:AllowDevelopmentUserHeader", true) && context.Request.Headers.TryGetValue("X-SEVPMS-Demo-Role", out var values) && (string.Equals(values.FirstOrDefault(), "Organizer", StringComparison.OrdinalIgnoreCase) || string.Equals(values.FirstOrDefault(), "Admin", StringComparison.OrdinalIgnoreCase));
    }
}
