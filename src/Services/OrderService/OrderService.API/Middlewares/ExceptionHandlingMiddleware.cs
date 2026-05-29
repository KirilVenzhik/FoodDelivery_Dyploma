using System.Text.Json;
using FluentValidation;
using OrderService.Domain.Exceptions;

namespace OrderService.API.Middlewares;

public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    private static readonly JsonSerializerOptions JsonOptions =
        new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

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
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, body) = exception switch
        {
            OrderNotFoundException e =>
                (StatusCodes.Status404NotFound, Error(e.Message)),

            OrderItemsEmptyException e =>
                (StatusCodes.Status400BadRequest, Error(e.Message)),

            RestaurantValidationException e =>
                (StatusCodes.Status422UnprocessableEntity, Error(e.Message)),

            InvalidOrderStatusTransitionException e =>
                (StatusCodes.Status409Conflict, Error(e.Message)),

            ValidationException e =>
                (StatusCodes.Status400BadRequest, ValidationError(e)),

            UnauthorizedAccessException e =>
                (StatusCodes.Status403Forbidden, Error(e.Message)),

            _ => (StatusCodes.Status500InternalServerError, Error("An unexpected error occurred."))
        };

        if (statusCode == StatusCodes.Status500InternalServerError)
            _logger.LogError(exception, "Unhandled exception");

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync(JsonSerializer.Serialize(body, JsonOptions));
    }

    private static object Error(string message) => new { error = message };

    private static object ValidationError(ValidationException ex) => new
    {
        error = "Validation failed.",
        errors = ex.Errors
            .GroupBy(f => f.PropertyName)
            .ToDictionary(
                g => g.Key,
                g => g.Select(f => f.ErrorMessage).ToArray())
    };
}
