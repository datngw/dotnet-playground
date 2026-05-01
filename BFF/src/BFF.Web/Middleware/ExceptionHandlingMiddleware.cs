namespace BFF.Web.Middleware;

// WHY: The frontend should never see raw .NET exception messages.
// The BFF catches all unhandled exceptions and returns a consistent error format,
// regardless of which downstream service caused the failure.
public class ExceptionHandlingMiddleware : IMiddleware
{
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(ILogger<ExceptionHandlingMiddleware> logger)
    {
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Downstream service call failed");
            context.Response.StatusCode = 502;
            await context.Response.WriteAsJsonAsync(new
            {
                Error = "One or more downstream services are unavailable",
                Detail = ex.Message
            });
        }
        catch (TaskCanceledException)
        {
            _logger.LogWarning("Request to downstream service timed out");
            context.Response.StatusCode = 504;
            await context.Response.WriteAsJsonAsync(new
            {
                Error = "Downstream service timed out"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception in BFF");
            context.Response.StatusCode = 500;
            await context.Response.WriteAsJsonAsync(new
            {
                Error = "An unexpected error occurred"
            });
        }
    }
}
