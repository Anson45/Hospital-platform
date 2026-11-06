using Microsoft.AspNetCore.Mvc;
using PatientAppointment.Application.Abstractions;
using PatientAppointment.Application.Departments;

namespace PatientAppointment.Api.Controllers;

[ApiController]
[Route("api/departments")]
public sealed class DepartmentsController : ControllerBase
{
    private readonly ICommandHandler<CreateDepartmentCommand, Guid> _createDepartment;

    public DepartmentsController(ICommandHandler<CreateDepartmentCommand, Guid> createDepartment)
    {
        _createDepartment = createDepartment;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateDepartmentCommand command, CancellationToken cancellationToken)
    {
        var departmentId = await _createDepartment.HandleAsync(command, cancellationToken);

        return StatusCode(StatusCodes.Status201Created, departmentId);
    }
}
