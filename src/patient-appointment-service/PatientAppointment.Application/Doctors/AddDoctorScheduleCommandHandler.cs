using PatientAppointment.Application.Abstractions;
using PatientAppointment.Application.Exceptions;
using PatientAppointment.Domain.Entities;

namespace PatientAppointment.Application.Doctors;

public sealed class AddDoctorScheduleCommandHandler : ICommandHandler<AddDoctorScheduleCommand>
{
    private readonly IDoctorRepository _doctors;
    private readonly IUnitOfWork _unitOfWork;

    public AddDoctorScheduleCommandHandler(IDoctorRepository doctors, IUnitOfWork unitOfWork)
    {
        _doctors = doctors;
        _unitOfWork = unitOfWork;
    }

    public async Task HandleAsync(AddDoctorScheduleCommand command, CancellationToken cancellationToken = default)
    {
        var doctor = await _doctors.GetByIdAsync(command.DoctorId, cancellationToken)
            ?? throw new NotFoundException(nameof(Doctor), command.DoctorId);

        doctor.AddSchedule(command.DayOfWeek, command.StartTime, command.EndTime);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
