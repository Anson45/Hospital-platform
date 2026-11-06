using PatientAppointment.Application.Abstractions;
using PatientAppointment.Domain.Entities;

namespace PatientAppointment.Application.Patients;

public sealed class RegisterPatientCommandHandler : ICommandHandler<RegisterPatientCommand, Guid>
{
    private readonly IPatientRepository _patients;
    private readonly IUnitOfWork _unitOfWork;
    private readonly TimeProvider _timeProvider;

    public RegisterPatientCommandHandler(
        IPatientRepository patients,
        IUnitOfWork unitOfWork,
        TimeProvider timeProvider)
    {
        _patients = patients;
        _unitOfWork = unitOfWork;
        _timeProvider = timeProvider;
    }

    public async Task<Guid> HandleAsync(RegisterPatientCommand command, CancellationToken cancellationToken = default)
    {
        var patient = new Patient(
            command.FirstName,
            command.LastName,
            command.DateOfBirth,
            _timeProvider.GetUtcNow(),
            command.Email,
            command.PhoneNumber);

        _patients.Add(patient);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return patient.Id;
    }
}
