using PatientAppointment.Application.Abstractions;
using PatientAppointment.Application.Exceptions;
using PatientAppointment.Domain.Entities;

namespace PatientAppointment.Application.Doctors;

public sealed class CreateDoctorCommandHandler : ICommandHandler<CreateDoctorCommand, Guid>
{
    private readonly IDoctorRepository _doctors;
    private readonly IDepartmentRepository _departments;
    private readonly IUnitOfWork _unitOfWork;

    public CreateDoctorCommandHandler(
        IDoctorRepository doctors,
        IDepartmentRepository departments,
        IUnitOfWork unitOfWork)
    {
        _doctors = doctors;
        _departments = departments;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> HandleAsync(CreateDoctorCommand command, CancellationToken cancellationToken = default)
    {
        _ = await _departments.GetByIdAsync(command.DepartmentId, cancellationToken)
            ?? throw new NotFoundException(nameof(Department), command.DepartmentId);

        var doctor = new Doctor(command.StaffUserId, command.DepartmentId);

        _doctors.Add(doctor);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return doctor.Id;
    }
}
