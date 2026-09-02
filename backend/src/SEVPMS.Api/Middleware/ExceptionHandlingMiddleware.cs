using System.Net;
using System.Text.Json;
using SEVPMS.Application.Common.Exceptions;

namespace SEVPMS.Api.Middleware;

public sealed class ExceptionHandlingMiddleware(
    RequestDelegate next,
    ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception exception)
        {
            await HandleExceptionAsync(
                context,
                exception);
        }
    }

    private async Task HandleExceptionAsync(
        HttpContext context,
        Exception exception)
    {
        var statusCode =
            exception switch
        {
            ArgumentException =>
                HttpStatusCode.BadRequest,

            UnauthorizedAccessException =>
                HttpStatusCode.Unauthorized,

            ForbiddenAccessException =>
                HttpStatusCode.Forbidden,

            KeyNotFoundException =>
                HttpStatusCode.NotFound,

            InvalidOperationException =>
                HttpStatusCode.Conflict,

            _ =>
                HttpStatusCode.InternalServerError
        };

        if (statusCode ==
            HttpStatusCode.InternalServerError)
        {
            logger.LogError(
                exception,
                "Unhandled server exception");
        }
        else
        {
            logger.LogWarning(
                "Request failed with {StatusCode}: {Message}",
                (int)statusCode,
                exception.Message);
        }

        context.Response.StatusCode =
            (int)statusCode;

        context.Response.ContentType =
            "application/json";

        var response =
            JsonSerializer.Serialize(
                new
                {
                    status = (int)statusCode,

                    message =
                        statusCode ==
                        HttpStatusCode.InternalServerError
                            ? "An unexpected server error occurred."
                            : exception.Message
                });

        await context.Response.WriteAsync(
            response);
    }
}