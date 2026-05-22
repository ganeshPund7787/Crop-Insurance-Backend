using System.Net;
using System.Text.Json;
using Authentication.Helpers;

namespace Authentication.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionMiddleware> logger)
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
            _logger.LogError(ex,
                "Unhandled exception on {Method} {Path}",
                context.Request.Method,
                context.Request.Path);

            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(
        HttpContext context,
        Exception exception)
    {
        // ─── Map exception type → HTTP status ─────────────────────────────
        var (statusCode, message) = exception switch
        {
            UnauthorizedAccessException =>
                (HttpStatusCode.Unauthorized, exception.Message),

            InvalidOperationException =>
                (HttpStatusCode.BadRequest, exception.Message),

            KeyNotFoundException =>
                (HttpStatusCode.NotFound, exception.Message),

            ArgumentException =>
                (HttpStatusCode.BadRequest, exception.Message),

            NotImplementedException =>
                (HttpStatusCode.NotImplemented,
                 "This feature is not yet implemented."),

            // Never expose internal errors to client in production
            _ => (HttpStatusCode.InternalServerError,
                  "An unexpected error occurred. Please try again later.")
        };

        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/json";

        var response = ApiResponse<object>.Fail(message);

        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }
}

// ─── Extension for clean Program.cs registration ───────────────────────────
public static class GlobalExceptionMiddlewareExtensions
{
    public static IApplicationBuilder UseGlobalExceptionHandler(
        this IApplicationBuilder app)
    {
        return app.UseMiddleware<GlobalExceptionMiddleware>();
    }
}