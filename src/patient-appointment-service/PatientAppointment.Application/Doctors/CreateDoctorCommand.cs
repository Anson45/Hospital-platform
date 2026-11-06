namespace PatientAppointment.Application.Doctors;

public sealed record CreateDoctorCommand(Guid StaffUserId, Guid DepartmentId);
