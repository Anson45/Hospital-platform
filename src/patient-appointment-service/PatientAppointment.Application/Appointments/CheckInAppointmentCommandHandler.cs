using Microsoft.Extensions.Logging;
using PatientAppointment.Application.Abstractions;
using PatientAppointment.Application.Exceptions;
using PatientAppointment.Domain.Entities;

namespace PatientAppointment.Application.Appointments;

/// <summary>
/// Marks the appointment as checked in and opens the visit record. Returns the new visit's id.
/// </summary>
public sealed partial class CheckInAppointmentCommandHandler : ICommandHandler<CheckInAppointmentCommand, Guid>
{
    private readonly IAppointmentRepository _appointments;
    private readonly IVisitRepository _visits;
    private readonly IUnitOfWork _unitOfWork;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<CheckInAppointmentCommandHandler> _logger;

    public CheckInAppointmentCommandHandler(
        IAppointmentRepository appointments,
        IVisitRepository visits,
        IUnitOfWork unitOfWork,
        TimeProvider timeProvider,
        ILogger<CheckInAppointmentCommandHandler> logger)
    {
        _appointments = appointments;
        _visits = visits;
        _unitOfWork = unitOfWork;
        _timeProvider = timeProvider;
        _logger = logger;
    }

    public async Task<Guid> HandleAsync(CheckInAppointmentCommand command, CancellationToken cancellationToken = default)
    {
        var appointment = await _appointments.GetByIdAsync(command.AppointmentId, cancellationToken)
            ?? throw new NotFoundException(nameof(Appointment), command.AppointmentId);

        appointment.CheckIn();

        var visit = new Visit(appointment.Id, appointment.PatientId, _timeProvider.GetUtcNow());
        _visits.Add(visit);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        LogCheckedIn(appointment.Id, visit.Id);

        return visit.Id;
    }

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Checked in appointment {AppointmentId}, opened visit {VisitId}")]
    private partial void LogCheckedIn(Guid appointmentId, Guid visitId);
}
