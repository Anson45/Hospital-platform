using PatientAppointment.Application.Abstractions;
using PatientAppointment.Application.Exceptions;
using PatientAppointment.Domain.Entities;

namespace PatientAppointment.Application.Appointments;

public sealed class GetPatientAppointmentsQueryHandler
    : IQueryHandler<GetPatientAppointmentsQuery, IReadOnlyList<AppointmentDto>>
{
    private readonly IPatientRepository _patients;
    private readonly IAppointmentRepository _appointments;

    public GetPatientAppointmentsQueryHandler(IPatientRepository patients, IAppointmentRepository appointments)
    {
        _patients = patients;
        _appointments = appointments;
    }

    public async Task<IReadOnlyList<AppointmentDto>> HandleAsync(
        GetPatientAppointmentsQuery query,
        CancellationToken cancellationToken = default)
    {
        _ = await _patients.GetByIdAsync(query.PatientId, cancellationToken)
            ?? throw new NotFoundException(nameof(Patient), query.PatientId);

        var appointments = await _appointments.GetByPatientIdAsync(query.PatientId, cancellationToken);

        return appointments
            .Select(appointment => new AppointmentDto(
                appointment.Id,
                appointment.DoctorId,
                appointment.ScheduledStart,
                appointment.ScheduledEnd,
                appointment.Status.ToString()))
            .ToList();
    }
}
