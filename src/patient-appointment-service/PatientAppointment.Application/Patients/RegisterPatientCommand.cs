namespace PatientAppointment.Application.Patients;

public sealed record RegisterPatientCommand(
    string FirstName,
    string LastName,
    DateOnly DateOfBirth,
    string? Email,
    string? PhoneNumber);
