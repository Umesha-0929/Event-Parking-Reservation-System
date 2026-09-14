namespace SEVPMS.Api.Middleware;

public sealed class CorrelationIdMiddleware(RequestDelegate next)
{
    private const string HeaderName = "X-Correlation-ID";

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId =
            context.Request.Headers.TryGetValue(HeaderName, out var value)
                ? value.ToString()
                : Guid.NewGuid().ToString("N");

        context.Response.Headers[HeaderName] = correlationId;
        context.Items[HeaderName] = correlationId;

        await next(context);
    }
}
