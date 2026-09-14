using System.Security.Claims;
using SEVPMS.Application.Features.Audit.Interfaces;

namespace SEVPMS.Api.Middleware;

public sealed class AuditLoggingMiddleware(RequestDelegate next, ILogger<AuditLoggingMiddleware> logger)
{
    private static readonly HashSet<string> MutatingMethods = new(StringComparer.OrdinalIgnoreCase)
    { HttpMethods.Post, HttpMethods.Put, HttpMethods.Patch, HttpMethods.Delete };

    public async Task InvokeAsync(HttpContext context, IAuditLogService auditLogService)
    {
        Exception? downstreamFailure = null;
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            downstreamFailure = ex;
            throw;
        }
        finally
        {
            if (MutatingMethods.Contains(context.Request.Method))
                await PersistAsync(context, auditLogService, downstreamFailure);
        }
    }

    private async Task PersistAsync(HttpContext context, IAuditLogService service, Exception? failure)
    {
        try
        {
            var raw = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
            Guid? actor = Guid.TryParse(raw, out var parsed) ? parsed : null;
            var path = context.Request.Path.Value ?? "/";
            var segments = path.Trim('/').Split('/', StringSplitOptions.RemoveEmptyEntries);
            var entityType = segments.Length >= 2 ? segments[1] : "api";
            string? entityId = segments.Length >= 3 && Guid.TryParse(segments[2], out var routeId)
                ? routeId.ToString() : null;
            var correlationId = context.Response.Headers.TryGetValue("X-Correlation-ID", out var values)
                ? values.FirstOrDefault() : context.TraceIdentifier;
            var outcome = failure is null ? $"HTTP {context.Response.StatusCode}" : $"FAILED {failure.GetType().Name}";

            // Use None so client disconnect/cancellation does not erase failure evidence.
            await service.WriteAsync(
                actor, $"{context.Request.Method} {path}", entityType, entityId,
                null, outcome, correlationId, context.Connection.RemoteIpAddress?.ToString(), CancellationToken.None);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to persist audit log.");
        }
    }
}
