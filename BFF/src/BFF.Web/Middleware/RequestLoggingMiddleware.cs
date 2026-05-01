namespace BFF.Web.Middleware;

// WHY: The BFF is the gateway between frontend and microservices.
// Logging request timing helps identify slow aggregations and bottlenecks.
public class RequestLoggingMiddleware : IMiddleware
{
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(ILogger<RequestLoggingMiddleware> logger)
    {
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var start = DateTime.UtcNow;
        var path = context.Request.Path;

        _logger.LogInformation("→ {Method} {Path}", context.Request.Method, path);

        await next(context);

        var elapsed = (DateTime.UtcNow - start).TotalMilliseconds;
        _logger.LogInformation("← {Method} {Path} → {StatusCode} ({Elapsed:F0}ms)",
            context.Request.Method, path, context.Response.StatusCode, elapsed);
    }
}
