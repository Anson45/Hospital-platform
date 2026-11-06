using PatientAppointment.Application.Abstractions;
using PatientAppointment.Domain.Entities;

namespace PatientAppointment.Application.Departments;

public sealed class CreateDepartmentCommandHandler : ICommandHandler<CreateDepartmentCommand, Guid>
{
    private readonly IDepartmentRepository _departments;
    private readonly IUnitOfWork _unitOfWork;

    public CreateDepartmentCommandHandler(IDepartmentRepository departments, IUnitOfWork unitOfWork)
    {
        _departments = departments;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> HandleAsync(CreateDepartmentCommand command, CancellationToken cancellationToken = default)
    {
        var department = new Department(command.Name);

        _departments.Add(department);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return department.Id;
    }
}
