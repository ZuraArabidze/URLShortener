using System.Net;
using System.Text.Json;
using URLShortener.API.DTOs;
using URLShortener.Core.Exceptions;

namespace URLShortener.API.Middleware;

public class ExceptionHandlingMiddleware
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
            _logger.LogError(ex, "An unhandled exception occurred while processing the request.");
            await HandleExceptionAsync(context, ex);
        }
    }

    public static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var response = new ErrorResponse
        {
            Message = exception.Message,
            Timestamp = DateTime.UtcNow
        };

        context.Response.StatusCode = exception switch
        {
            UrlNotFoundException => (int)HttpStatusCode.NotFound,
            ShortCodeAlreadyExistsException => (int)HttpStatusCode.Conflict,
            InvalidUrlException => (int)HttpStatusCode.BadRequest,
            UrlExpiredException => (int)HttpStatusCode.Gone,
            UrlShortenerException => (int)HttpStatusCode.BadRequest,
            _ => (int)HttpStatusCode.InternalServerError
        };

        if(context.Response.StatusCode == (int)HttpStatusCode.InternalServerError)
        {
            response.Message = "An internal server error occurred.";
            response.Details = exception.Message;
        }

        var jsonResponse = JsonSerializer.Serialize(response);
        await context.Response.WriteAsync(jsonResponse);
    }
}
