using CaraDog.Core.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace CaraDog.Api.Middleware;

public sealed class ProblemDetailsMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ProblemDetailsMiddleware> _logger;

    public ProblemDetailsMiddleware(RequestDelegate next, ILogger<ProblemDetailsMiddleware> logger)
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

    private Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, code) = exception switch
        {
            NotFoundException => (StatusCodes.Status404NotFound, "HBH-API-404"),
            ValidationException => (StatusCodes.Status400BadRequest, "HBH-API-400"),
            ConflictException => (StatusCodes.Status409Conflict, "HBH-API-409"),
            _ => (StatusCodes.Status500InternalServerError, "HBH-API-500")
        };

        if (statusCode == StatusCodes.Status500InternalServerError)
        {
            _logger.LogError(exception, "{Code} Unhandled exception", code);
        }
        else
        {
            _logger.LogWarning(exception, "{Code} Request failed", code);
        }

        var problem = new ProblemDetails
        {
            Status = statusCode,
            Title = exception.Message,
            Instance = context.Request.Path
        };

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/problem+json";
        return context.Response.WriteAsJsonAsync(problem);
    }
}
