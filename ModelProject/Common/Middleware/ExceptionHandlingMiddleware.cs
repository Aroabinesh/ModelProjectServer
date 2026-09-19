using System.Net;
using System.Text.Json;
using ModelProject.Common.Exceptions;

namespace ModelProject.Common.Middleware;

// Single place that turns exceptions into consistent application/problem+json responses, so
// controllers stay thin and don't need repeated try/catch blocks per action.
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, title) = exception switch
        {
            NotFoundException => (HttpStatusCode.NotFound, "Resource not found"),
            UnauthorizedException => (HttpStatusCode.Unauthorized, "Unauthorized"),
            InvalidStatusTransitionException => (HttpStatusCode.Conflict, "Invalid status transition"),
            ConcurrencyConflictException => (HttpStatusCode.Conflict, "Concurrency conflict"),
            BusinessRuleException => (HttpStatusCode.BadRequest, "Business rule violation"),
            ArgumentException => (HttpStatusCode.BadRequest, "Invalid argument"),
            _ => (HttpStatusCode.InternalServerError, "An unexpected error occurred")
        };

        if (statusCode == HttpStatusCode.InternalServerError)
        {
            _logger.LogError(exception, "Unhandled exception while processing {Method} {Path}",
                context.Request.Method, context.Request.Path);
        }
        else
        {
            _logger.LogWarning(exception, "Handled exception ({StatusCode}) while processing {Method} {Path}: {Message}",
                (int)statusCode, context.Request.Method, context.Request.Path, exception.Message);
        }

        var problemDetails = new
        {
            type = $"https://httpstatuses.io/{(int)statusCode}",
            title,
            status = (int)statusCode,
            detail = statusCode == HttpStatusCode.InternalServerError
                ? "An unexpected error occurred. Please try again later."
                : exception.Message,
            traceId = context.TraceIdentifier
        };

        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = (int)statusCode;
        await context.Response.WriteAsync(JsonSerializer.Serialize(problemDetails));
    }
}
