using PatientAppointment.Domain.Entities;

namespace PatientAppointment.Application.Abstractions;

public interface IPatientRepository
{
    Task<Patient?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    void Add(Patient patient);
}
