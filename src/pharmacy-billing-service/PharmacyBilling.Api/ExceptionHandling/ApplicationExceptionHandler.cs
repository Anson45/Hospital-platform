using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PharmacyBilling.Application.Exceptions;
using PharmacyBilling.Domain.Exceptions;

namespace PharmacyBilling.Api.ExceptionHandling;

/// <summary>
/// Maps domain, application, and concurrency failures to ProblemDetails responses, so controllers stay free of try/catch.
/// </summary>
public sealed partial class ApplicationExceptionHandler : IExceptionHandler
{
    private readonly ILogger<ApplicationExceptionHandler> _logger;

    public ApplicationExceptionHandler(ILogger<ApplicationExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (statusCode, title, detail) = exception switch
        {
            NotFoundException => (StatusCodes.Status404NotFound, "Resource not found", exception.Message),
            DomainException => (StatusCodes.Status422UnprocessableEntity, "Business rule violated", exception.Message),
            DbUpdateConcurrencyException => (
                StatusCodes.Status409Conflict,
                "Concurrent update",
                "The record was changed by another request. Reload it and try again."),
            ArgumentException => (StatusCodes.Status400BadRequest, "Invalid request", exception.Message),
            _ => (0, string.Empty, string.Empty)
        };

        if (statusCode == 0)
        {
            return false;
        }

        // Expected client errors are logged as warnings. Anything unexpected is left to the default handler.
        LogRejected(httpContext.Request.Method, httpContext.Request.Path, statusCode, detail);

        httpContext.Response.StatusCode = statusCode;

        await httpContext.Response.WriteAsJsonAsync(
            new ProblemDetails
            {
                Status = statusCode,
                Title = title,
                Detail = detail
            },
            cancellationToken);

        return true;
    }

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Request {Method} {Path} rejected with {StatusCode}: {Reason}")]
    private partial void LogRejected(string method, string path, int statusCode, string reason);
}
