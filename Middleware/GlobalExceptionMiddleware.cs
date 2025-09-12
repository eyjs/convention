using System.Net;
using System.Text.Json;

namespace LocalRAG.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
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
            _logger.LogError(ex, "An unhandled exception occurred");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var response = context.Response;
        response.ContentType = "application/json";

        var errorResponse = exception switch
        {
            ArgumentNullException => new ErrorResponse("Bad Request", exception.Message, (int)HttpStatusCode.BadRequest),
            ArgumentException => new ErrorResponse("Bad Request", exception.Message, (int)HttpStatusCode.BadRequest),
            InvalidOperationException => new ErrorResponse("Internal Server Error", exception.Message, (int)HttpStatusCode.InternalServerError),
            NotImplementedException => new ErrorResponse("Not Implemented", exception.Message, (int)HttpStatusCode.NotImplemented),
            UnauthorizedAccessException => new ErrorResponse("Unauthorized", exception.Message, (int)HttpStatusCode.Unauthorized),
            _ => new ErrorResponse("Internal Server Error", "An unexpected error occurred", (int)HttpStatusCode.InternalServerError)
        };

        response.StatusCode = errorResponse.StatusCode;

        var jsonResponse = JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await response.WriteAsync(jsonResponse);
    }
}

public record ErrorResponse(string Title, string Message, int StatusCode);
