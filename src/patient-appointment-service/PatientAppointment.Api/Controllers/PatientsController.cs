using Microsoft.AspNetCore.Mvc;
using PatientAppointment.Application.Abstractions;
using PatientAppointment.Application.Appointments;
using PatientAppointment.Application.Patients;

namespace PatientAppointment.Api.Controllers;

[ApiController]
[Route("api/patients")]
public sealed class PatientsController : ControllerBase
{
    private readonly ICommandHandler<RegisterPatientCommand, Guid> _registerPatient;
    private readonly IQueryHandler<GetPatientAppointmentsQuery, IReadOnlyList<AppointmentDto>> _getAppointments;

    public PatientsController(
        ICommandHandler<RegisterPatientCommand, Guid> registerPatient,
        IQueryHandler<GetPatientAppointmentsQuery, IReadOnlyList<AppointmentDto>> getAppointments)
    {
        _registerPatient = registerPatient;
        _getAppointments = getAppointments;
    }

    [HttpPost]
    public async Task<IActionResult> Register([FromBody] RegisterPatientCommand command, CancellationToken cancellationToken)
    {
        var patientId = await _registerPatient.HandleAsync(command, cancellationToken);

        return StatusCode(StatusCodes.Status201Created, patientId);
    }

    [HttpGet("{patientId:guid}/appointments")]
    public async Task<ActionResult<IReadOnlyList<AppointmentDto>>> GetAppointments(
        Guid patientId,
        CancellationToken cancellationToken)
    {
        var appointments = await _getAppointments.HandleAsync(
            new GetPatientAppointmentsQuery(patientId),
            cancellationToken);

        return Ok(appointments);
    }
}
