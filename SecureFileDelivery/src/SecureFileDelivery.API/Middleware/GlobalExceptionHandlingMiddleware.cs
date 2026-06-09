using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using SecureFileDelivery.Domain.Exceptions;

namespace SecureFileDelivery.API.Middleware;

public sealed class GlobalExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger;

    public GlobalExceptionHandlingMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Request processing failed.");
            await WriteProblemDetailsAsync(context, exception);
        }
    }

    private static async Task WriteProblemDetailsAsync(HttpContext context, Exception exception)
    {
        var (status, title) = exception switch
        {
            TokenExpiredException => (StatusCodes.Status410Gone, "Token expired"),
            TokenNotFoundException => (StatusCodes.Status404NotFound, "Token not found"),
            TokenRevokedException => (StatusCodes.Status410Gone, "Token revoked"),
            TokenAlreadyUsedException => (StatusCodes.Status410Gone, "Token already used"),
            StatementNotFoundException => (StatusCodes.Status404NotFound, "Statement not found"),
            InvalidFileTypeException => (StatusCodes.Status415UnsupportedMediaType, "Invalid file type"),
            ValidationException => (StatusCodes.Status400BadRequest, "Validation failed"),
            _ => (StatusCodes.Status500InternalServerError, "Internal server error")
        };

        var problemDetails = new ProblemDetails
        {
            Title = title,
            Detail = exception.Message,
            Status = status,
            Instance = context.Request.Path
        };

        if (exception is ValidationException validationException)
        {
            problemDetails.Extensions["errors"] = validationException.Errors
                .GroupBy(x => x.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(x => x.ErrorMessage).ToArray());
        }

        context.Response.StatusCode = status;
        context.Response.ContentType = "application/problem+json";
        await context.Response.WriteAsJsonAsync(problemDetails);
    }
}
