using FluentValidation;
using NomiWrite.Auth.Application.Exceptions;

namespace NomiWrite.Auth.API.Middleware;

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
        catch (Exception exception)
        {
            await HandleExceptionAsync(context, exception);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        int statusCode;
        string message;
        string[] errors;

        switch (exception)
        {
            case ValidationException validationException:
                statusCode = StatusCodes.Status400BadRequest;
                message = "Validation failed.";
                errors = validationException.Errors.Select(e => e.ErrorMessage).ToArray();
                break;

            case UserAlreadyExistsException:
                statusCode = StatusCodes.Status409Conflict;
                message = exception.Message;
                errors = Array.Empty<string>();
                break;

            case InvalidCredentialsException:
            case InvalidRefreshTokenException:
                statusCode = StatusCodes.Status401Unauthorized;
                message = exception.Message;
                errors = Array.Empty<string>();
                break;

            default:
                statusCode = StatusCodes.Status500InternalServerError;
                message = "An unexpected error occurred.";
                errors = Array.Empty<string>();
                _logger.LogError(exception, "Unhandled exception for request {Method} {Path}",
                    context.Request.Method, context.Request.Path);
                break;
        }

        if (context.Response.HasStarted)
            throw exception;

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(new { success = false, message, errors });
    }
}