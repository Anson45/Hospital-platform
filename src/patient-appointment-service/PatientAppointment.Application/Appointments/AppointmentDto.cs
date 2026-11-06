namespace PatientAppointment.Application.Appointments;

public sealed record AppointmentDto(
    Guid Id,
    Guid DoctorId,
    DateTimeOffset ScheduledStart,
    DateTimeOffset ScheduledEnd,
    string Status);
