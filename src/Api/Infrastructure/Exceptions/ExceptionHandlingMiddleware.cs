using System.Text.Json;

namespace Api.Infrastructure.Exceptions;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _logger = logger;
        _next = next;
    }
    
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Validation error. RequestId: {RequestId}", context.TraceIdentifier);

            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            context.Response.ContentType = "application/json; charset=utf-8";

            var payload = new
            {
                error = "validation_error",
                message = ex.Message,
                requestId = context.TraceIdentifier,
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(payload));
        }
        catch (InvalidOperationException ex) when (ex.Message == "EMAIL_ALREADY_REGISTERED" ||
                                                   ex.Message == "CPF_ALREADY_REGISTERED")
        {
            _logger.LogWarning(ex, "Business conflict. Error: {Error}. RequestId: {RequestId}", ex.Message,
                context.TraceIdentifier);

            context.Response.StatusCode = StatusCodes.Status409Conflict;
            context.Response.ContentType = "application/json; charset=utf-8";

            var payload = new
            {
                error = "conflict",
                message = ex.Message,
                requestId = context.TraceIdentifier,
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(payload));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception. RequestId: {RequestId}", context.TraceIdentifier);
            
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/json; charset=utf-8";
            
            var payload = new
            {
                error = "internal_server_error",
                message = "An unhandled error occurred.",
                requestId = context.TraceIdentifier,
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(payload));
        }
    }
}