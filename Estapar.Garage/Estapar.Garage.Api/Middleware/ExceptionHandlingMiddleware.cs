namespace Estapar.Garage.Api.Middleware;

/// <summary>Maps domain validation exceptions to HTTP status codes for every request.</summary>
public sealed class ExceptionHandlingMiddleware
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
            var (statusCode, message) = MapException(ex);
            _logger.LogWarning(ex, "Request {Method} {Path} failed: {Message}", context.Request.Method, context.Request.Path, ex.Message);

            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(new { error = message });
        }
    }

    private static (int StatusCode, string Message) MapException(Exception ex) => ex switch
    {
        ArgumentException e => (StatusCodes.Status400BadRequest, e.Message),
        InvalidOperationException e => (StatusCodes.Status400BadRequest, e.Message),
        _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred.")
    };
}
