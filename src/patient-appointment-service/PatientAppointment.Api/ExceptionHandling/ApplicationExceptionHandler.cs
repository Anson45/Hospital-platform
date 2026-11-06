using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PatientAppointment.Application.Exceptions;
using PatientAppointment.Domain.Exceptions;

namespace PatientAppointment.Api.ExceptionHandling;

/// <summary>
/// Maps domain and application failures to ProblemDetails responses, so controllers stay free of try/catch.
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
        var (statusCode, title) = exception switch
        {
            NotFoundException => (StatusCodes.Status404NotFound, "Resource not found"),
            DomainException => (StatusCodes.Status422UnprocessableEntity, "Business rule violated"),
            ArgumentException => (StatusCodes.Status400BadRequest, "Invalid request"),
            _ => (0, string.Empty)
        };

        if (statusCode == 0)
        {
            return false;
        }

        // Expected client errors are logged as warnings. Anything unexpected is left to the default handler.
        LogRejected(httpContext.Request.Method, httpContext.Request.Path, statusCode, exception.Message);

        httpContext.Response.StatusCode = statusCode;

        await httpContext.Response.WriteAsJsonAsync(
            new ProblemDetails
            {
                Status = statusCode,
                Title = title,
                Detail = exception.Message
            },
            cancellationToken);

        return true;
    }

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Request {Method} {Path} rejected with {StatusCode}: {Reason}")]
    private partial void LogRejected(string method, string path, int statusCode, string reason);
}
