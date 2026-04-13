using FluentValidation;
using ICMarkets.Api.Serialization;

namespace ICMarkets.Api.Middleware;

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
        catch (ValidationException ex)
        {
            _logger.LogWarning("Validation failed: {Errors}", ex.Message);
            context.Response.StatusCode = StatusCodes.Status400BadRequest;

            var body = new ValidationErrorResponse(
                "Validation Failed",
                400,
                ex.Errors.Select(e => new ValidationFieldError(e.PropertyName, e.ErrorMessage)));

            await context.Response.WriteAsJsonAsync(
                body, ApiJsonContext.Default.ValidationErrorResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception");
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;

            await context.Response.WriteAsJsonAsync(
                new ErrorResponse("Internal Server Error", 500),
                ApiJsonContext.Default.ErrorResponse);
        }
    }
}
