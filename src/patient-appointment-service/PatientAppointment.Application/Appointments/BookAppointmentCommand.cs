namespace PatientAppointment.Application.Appointments;

public sealed record BookAppointmentCommand(
    Guid PatientId,
    Guid DoctorId,
    DateTimeOffset ScheduledStart,
    TimeSpan Duration);
