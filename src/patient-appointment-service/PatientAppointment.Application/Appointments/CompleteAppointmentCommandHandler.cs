using PatientAppointment.Application.Abstractions;
using PatientAppointment.Application.Exceptions;
using PatientAppointment.Domain.Entities;
using PatientAppointment.Domain.Exceptions;

namespace PatientAppointment.Application.Appointments;

/// <summary>
/// Completes a checked-in appointment and closes its visit. Completing the visit is what later triggers billing.
/// </summary>
public sealed class CompleteAppointmentCommandHandler : ICommandHandler<CompleteAppointmentCommand>
{
    private readonly IAppointmentRepository _appointments;
    private readonly IVisitRepository _visits;
    private readonly IUnitOfWork _unitOfWork;
    private readonly TimeProvider _timeProvider;

    public CompleteAppointmentCommandHandler(
        IAppointmentRepository appointments,
        IVisitRepository visits,
        IUnitOfWork unitOfWork,
        TimeProvider timeProvider)
    {
        _appointments = appointments;
        _visits = visits;
        _unitOfWork = unitOfWork;
        _timeProvider = timeProvider;
    }

    public async Task HandleAsync(CompleteAppointmentCommand command, CancellationToken cancellationToken = default)
    {
        var appointment = await _appointments.GetByIdAsync(command.AppointmentId, cancellationToken)
            ?? throw new NotFoundException(nameof(Appointment), command.AppointmentId);

        var visit = await _visits.GetOpenByAppointmentIdAsync(command.AppointmentId, cancellationToken)
            ?? throw new DomainException("No open visit exists for this appointment.");

        appointment.Complete();
        visit.End(_timeProvider.GetUtcNow());
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
