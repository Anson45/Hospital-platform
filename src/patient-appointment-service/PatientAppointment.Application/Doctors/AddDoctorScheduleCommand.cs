namespace PatientAppointment.Application.Doctors;

public sealed record AddDoctorScheduleCommand(
    Guid DoctorId,
    DayOfWeek DayOfWeek,
    TimeOnly StartTime,
    TimeOnly EndTime);
