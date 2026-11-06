namespace PatientAppointment.Api.Requests;

/// <summary>
/// Body for adding a schedule block. The doctor id comes from the route, so it is not part of the body.
/// </summary>
public sealed record AddDoctorScheduleRequest(DayOfWeek DayOfWeek, TimeOnly StartTime, TimeOnly EndTime);
