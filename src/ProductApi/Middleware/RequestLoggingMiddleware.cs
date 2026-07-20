namespace ProductApi.Middleware;

public sealed class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(
        RequestDelegate next,
        ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var startedAt = DateTime.UtcNow;

        _logger.LogInformation(
            "Request started: {Method} {Path}{QueryString} at {StartedAtUtc}.",
            context.Request.Method,
            context.Request.Path,
            context.Request.QueryString,
            startedAt);

        try
        {
            await _next(context);
        }
        finally
        {
            var elapsedMilliseconds =
                (DateTime.UtcNow - startedAt).TotalMilliseconds;

            _logger.LogInformation(
                "Request completed: {Method} {Path} responded {StatusCode} "
                + "in {ElapsedMilliseconds:F2} ms.",
                context.Request.Method,
                context.Request.Path,
                context.Response.StatusCode,
                elapsedMilliseconds);
        }
    }
}
