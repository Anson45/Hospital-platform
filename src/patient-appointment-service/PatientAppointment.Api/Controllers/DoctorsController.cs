using Microsoft.AspNetCore.Mvc;
using PatientAppointment.Api.Requests;
using PatientAppointment.Application.Abstractions;
using PatientAppointment.Application.Doctors;

namespace PatientAppointment.Api.Controllers;

[ApiController]
[Route("api/doctors")]
public sealed class DoctorsController : ControllerBase
{
    private readonly ICommandHandler<CreateDoctorCommand, Guid> _createDoctor;
    private readonly ICommandHandler<AddDoctorScheduleCommand> _addSchedule;

    public DoctorsController(
        ICommandHandler<CreateDoctorCommand, Guid> createDoctor,
        ICommandHandler<AddDoctorScheduleCommand> addSchedule)
    {
        _createDoctor = createDoctor;
        _addSchedule = addSchedule;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateDoctorCommand command, CancellationToken cancellationToken)
    {
        var doctorId = await _createDoctor.HandleAsync(command, cancellationToken);

        return StatusCode(StatusCodes.Status201Created, doctorId);
    }

    [HttpPost("{doctorId:guid}/schedules")]
    public async Task<IActionResult> AddSchedule(
        Guid doctorId,
        [FromBody] AddDoctorScheduleRequest request,
        CancellationToken cancellationToken)
    {
        await _addSchedule.HandleAsync(
            new AddDoctorScheduleCommand(doctorId, request.DayOfWeek, request.StartTime, request.EndTime),
            cancellationToken);

        return NoContent();
    }
}
