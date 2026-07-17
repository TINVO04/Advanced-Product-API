using ProductApi.Common.Exceptions;
using ProductApi.Common.Responses;

namespace ProductApi.Middleware;

public sealed class GlobalExceptionMiddleware
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
        catch (Exception exception)
        {
            await HandleExceptionAsync(context, exception);
        }
    }

    private async Task HandleExceptionAsync(
        HttpContext context,
        Exception exception)
    {
        var (statusCode, message, errors) = exception switch
        {
            NotFoundException => (
                StatusCodes.Status404NotFound,
                exception.Message,
                null),
            BadRequestException badRequestException => (
                StatusCodes.Status400BadRequest,
                badRequestException.Message,
                badRequestException.Errors),
            ConflictException => (
                StatusCodes.Status409Conflict,
                exception.Message,
                null),
            _ => (
                StatusCodes.Status500InternalServerError,
                "An unexpected error occurred.",
                null),
        };

        if (statusCode >= StatusCodes.Status500InternalServerError)
        {
            _logger.LogError(
                exception,
                "Unhandled exception while processing {Method} {Path}.",
                context.Request.Method,
                context.Request.Path);
        }
        else
        {
            _logger.LogWarning(
                exception,
                "Request failed with status code {StatusCode} for {Method} {Path}.",
                statusCode,
                context.Request.Method,
                context.Request.Path);
        }

        var response = new ApiResponse<object>
        {
            Success = false,
            Message = message,
            Errors = errors
        };

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        await context.Response.WriteAsJsonAsync(response);
    }
}
