namespace Authentication.Middleware;

// ─── Adds security headers to every response ──────────────────────────────
// Protects against XSS, clickjacking, MIME sniffing, and more
public class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;

    public SecurityHeadersMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Remove server identification
        context.Response.Headers.Remove("Server");
        context.Response.Headers.Remove("X-Powered-By");

        // Prevent MIME type sniffing
        context.Response.Headers
            .Append("X-Content-Type-Options", "nosniff");

        // Prevent clickjacking
        context.Response.Headers
            .Append("X-Frame-Options", "DENY");

        // XSS protection for older browsers
        context.Response.Headers
            .Append("X-XSS-Protection", "1; mode=block");

        // Referrer policy
        context.Response.Headers
            .Append("Referrer-Policy", "strict-origin-when-cross-origin");

        // Content Security Policy
        context.Response.Headers
            .Append("Content-Security-Policy", "default-src 'self'");

        // HSTS — force HTTPS for 1 year
        if (context.Request.IsHttps)
        {
            context.Response.Headers
                .Append("Strict-Transport-Security",
                    "max-age=31536000; includeSubDomains");
        }

        await _next(context);
    }
}

public static class SecurityHeadersMiddlewareExtensions
{
    public static IApplicationBuilder UseSecurityHeaders(
        this IApplicationBuilder app)
    {
        return app.UseMiddleware<SecurityHeadersMiddleware>();
    }
}