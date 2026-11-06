using Microsoft.AspNetCore.Mvc;
using PatientAppointment.Application.Abstractions;
using PatientAppointment.Application.Appointments;

namespace PatientAppointment.Api.Controllers;

[ApiController]
[Route("api/appointments")]
public sealed class AppointmentsController : ControllerBase
{
    private readonly ICommandHandler<BookAppointmentCommand, Guid> _book;
    private readonly ICommandHandler<CheckInAppointmentCommand, Guid> _checkIn;
    private readonly ICommandHandler<CompleteAppointmentCommand> _complete;

    public AppointmentsController(
        ICommandHandler<BookAppointmentCommand, Guid> book,
        ICommandHandler<CheckInAppointmentCommand, Guid> checkIn,
        ICommandHandler<CompleteAppointmentCommand> complete)
    {
        _book = book;
        _checkIn = checkIn;
        _complete = complete;
    }

    [HttpPost]
    public async Task<IActionResult> Book([FromBody] BookAppointmentCommand command, CancellationToken cancellationToken)
    {
        var appointmentId = await _book.HandleAsync(command, cancellationToken);

        return StatusCode(StatusCodes.Status201Created, appointmentId);
    }

    /// <summary>
    /// Returns the id of the visit opened by this check-in.
    /// </summary>
    [HttpPost("{appointmentId:guid}/check-in")]
    public async Task<IActionResult> CheckIn(Guid appointmentId, CancellationToken cancellationToken)
    {
        var visitId = await _checkIn.HandleAsync(new CheckInAppointmentCommand(appointmentId), cancellationToken);

        return StatusCode(StatusCodes.Status201Created, visitId);
    }

    [HttpPost("{appointmentId:guid}/complete")]
    public async Task<IActionResult> Complete(Guid appointmentId, CancellationToken cancellationToken)
    {
        await _complete.HandleAsync(new CompleteAppointmentCommand(appointmentId), cancellationToken);

        return NoContent();
    }
}
