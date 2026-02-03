using System.Diagnostics;

namespace URLShortener.API.Middleware;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();

        var requestPath = context.Request.Path;
        var requestMethod = context.Request.Method;

        _logger.LogInformation("Incoming request: {Method} {Path}", requestMethod, requestPath);

        try
        {
            await _next(context);
        }
        finally
        {
            stopwatch.Stop();
            _logger.LogInformation(
                "Request {Method} {Path} completed with status {StatusCode} in {ElapsedMilliseconds}ms",
                requestMethod,
                requestPath,
                context.Response.StatusCode,
                stopwatch.ElapsedMilliseconds);
        }
    }
}
