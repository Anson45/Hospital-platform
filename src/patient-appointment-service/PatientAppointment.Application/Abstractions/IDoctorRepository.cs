using PatientAppointment.Domain.Entities;

namespace PatientAppointment.Application.Abstractions;

public interface IDoctorRepository
{
    /// <summary>
    /// Loads the doctor together with its schedule blocks.
    /// </summary>
    Task<Doctor?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    void Add(Doctor doctor);
}
