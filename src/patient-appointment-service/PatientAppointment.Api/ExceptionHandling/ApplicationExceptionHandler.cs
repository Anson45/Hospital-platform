using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using PatientAppointment.Application.Exceptions;
using PatientAppointment.Domain.Exceptions;

namespace PatientAppointment.Api.ExceptionHandling;

/// <summary>
/// Maps domain and application failures to ProblemDetails responses, so controllers stay free of try/catch.
/// </summary>
public sealed class ApplicationExceptionHandler : IExceptionHandler
{
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
}
