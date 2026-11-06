using Microsoft.Extensions.Logging;
using PatientAppointment.Application.Abstractions;
using PatientAppointment.Application.Exceptions;
using PatientAppointment.Domain.Entities;
using PatientAppointment.Domain.Exceptions;

namespace PatientAppointment.Application.Appointments;

/// <summary>
/// Completes a checked-in appointment and closes its visit. Completing the visit is what later triggers billing.
/// </summary>
public sealed partial class CompleteAppointmentCommandHandler : ICommandHandler<CompleteAppointmentCommand>
{
    private readonly IAppointmentRepository _appointments;
    private readonly IVisitRepository _visits;
    private readonly IUnitOfWork _unitOfWork;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<CompleteAppointmentCommandHandler> _logger;

    public CompleteAppointmentCommandHandler(
        IAppointmentRepository appointments,
        IVisitRepository visits,
        IUnitOfWork unitOfWork,
        TimeProvider timeProvider,
        ILogger<CompleteAppointmentCommandHandler> logger)
    {
        _appointments = appointments;
        _visits = visits;
        _unitOfWork = unitOfWork;
        _timeProvider = timeProvider;
        _logger = logger;
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

        LogCompleted(appointment.Id, visit.Id);
    }

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Completed appointment {AppointmentId}, closed visit {VisitId}")]
    private partial void LogCompleted(Guid appointmentId, Guid visitId);
}
