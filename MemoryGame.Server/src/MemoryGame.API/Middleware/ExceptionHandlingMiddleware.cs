using System.Net;
using System.Text.Json;
using FluentValidation;
using MemoryGame.Domain.Common;

namespace MemoryGame.API.Middleware;

/// <summary>
/// Global exception handling middleware that maps application exceptions to
/// appropriate HTTP responses with structured error bodies.
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    /// <summary>
    /// Initializes the middleware with the request pipeline and logger.
    /// </summary>
    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ValidationException ex)
        {
            await HandleValidationExceptionAsync(context, ex);
        }
        catch (DomainException ex)
        {
            await HandleDomainExceptionAsync(context, ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception occurred.");
            await HandleUnexpectedExceptionAsync(context);
        }
    }

    private static async Task HandleValidationExceptionAsync(HttpContext context, ValidationException ex)
    {
        context.Response.StatusCode = (int)HttpStatusCode.UnprocessableEntity;
        context.Response.ContentType = "application/json";

        var errors = ex.Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(
                g => g.Key,
                g => g.Select(e => e.ErrorMessage).ToArray());

        var body = new { errors };
        await context.Response.WriteAsync(JsonSerializer.Serialize(body, JsonOptions));
    }

    private static async Task HandleDomainExceptionAsync(HttpContext context, DomainException ex)
    {
        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
        context.Response.ContentType = "application/json";

        var body = new { errorCode = ex.ErrorCode, message = ex.Message };
        await context.Response.WriteAsync(JsonSerializer.Serialize(body, JsonOptions));
    }

    private static async Task HandleUnexpectedExceptionAsync(HttpContext context)
    {
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
        context.Response.ContentType = "application/json";

        var body = new { errorCode = "INTERNAL_ERROR", message = "An unexpected error occurred." };
        await context.Response.WriteAsync(JsonSerializer.Serialize(body, JsonOptions));
    }
}
