using PatientAppointment.Application.Abstractions;
using PatientAppointment.Application.Exceptions;
using PatientAppointment.Domain.Entities;
using PatientAppointment.Domain.Exceptions;

namespace PatientAppointment.Application.Appointments;

public sealed class BookAppointmentCommandHandler : ICommandHandler<BookAppointmentCommand, Guid>
{
    private readonly IPatientRepository _patients;
    private readonly IDoctorRepository _doctors;
    private readonly IAppointmentRepository _appointments;
    private readonly IUnitOfWork _unitOfWork;

    public BookAppointmentCommandHandler(
        IPatientRepository patients,
        IDoctorRepository doctors,
        IAppointmentRepository appointments,
        IUnitOfWork unitOfWork)
    {
        _patients = patients;
        _doctors = doctors;
        _appointments = appointments;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> HandleAsync(BookAppointmentCommand command, CancellationToken cancellationToken = default)
    {
        _ = await _patients.GetByIdAsync(command.PatientId, cancellationToken)
            ?? throw new NotFoundException(nameof(Patient), command.PatientId);

        var doctor = await _doctors.GetByIdAsync(command.DoctorId, cancellationToken)
            ?? throw new NotFoundException(nameof(Doctor), command.DoctorId);

        if (!doctor.IsWorkingAt(command.ScheduledStart, command.Duration))
        {
            throw new DomainException("The doctor is not working at the requested time.");
        }

        var scheduledEnd = command.ScheduledStart + command.Duration;
        var isSlotTaken = await _appointments.HasOverlapAsync(
            command.DoctorId,
            command.ScheduledStart,
            scheduledEnd,
            cancellationToken);

        if (isSlotTaken)
        {
            throw new DomainException("The doctor already has an appointment in that time slot.");
        }

        var appointment = new Appointment(
            command.PatientId,
            command.DoctorId,
            command.ScheduledStart,
            command.Duration);

        _appointments.Add(appointment);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return appointment.Id;
    }
}
