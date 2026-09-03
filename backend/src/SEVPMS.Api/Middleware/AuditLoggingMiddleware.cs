using System.Security.Claims;
using SEVPMS.Application.Features.Audit.Interfaces;

namespace SEVPMS.Api.Middleware;

public sealed class AuditLoggingMiddleware(
    RequestDelegate next,
    ILogger<AuditLoggingMiddleware> logger)
{
    private static readonly HashSet<string> MutatingMethods =
        new(StringComparer.OrdinalIgnoreCase)
        {
            HttpMethods.Post,
            HttpMethods.Put,
            HttpMethods.Patch,
            HttpMethods.Delete
        };

    public async Task InvokeAsync(HttpContext context, IAuditLogService auditLogService)
    {
        await next(context);

        if (!MutatingMethods.Contains(context.Request.Method))
            return;

        try
        {
            var raw = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
            Guid? actor = Guid.TryParse(raw, out var parsed) ? parsed : null;

            var path = context.Request.Path.Value ?? "/";
            var segments = path.Trim('/').Split('/', StringSplitOptions.RemoveEmptyEntries);
            var entityType = segments.Length >= 2 ? segments[1] : "api";

            string? entityId = null;
            if (segments.Length >= 3 && Guid.TryParse(segments[2], out var routeId))
                entityId = routeId.ToString();

            var correlationId =
                context.Response.Headers.TryGetValue("X-Correlation-ID", out var values)
                    ? values.FirstOrDefault()
                    : context.TraceIdentifier;

            await auditLogService.WriteAsync(
                actor,
                $"{context.Request.Method} {path}",
                entityType,
                entityId,
                null,
                $"HTTP {context.Response.StatusCode}",
                correlationId,
                context.Connection.RemoteIpAddress?.ToString(),
                context.RequestAborted);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to persist audit log.");
        }
    }
}
