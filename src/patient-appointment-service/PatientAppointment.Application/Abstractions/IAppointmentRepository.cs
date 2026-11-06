using PatientAppointment.Domain.Entities;

namespace PatientAppointment.Application.Abstractions;

public interface IAppointmentRepository
{
    Task<Appointment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Appointment>> GetByPatientIdAsync(Guid patientId, CancellationToken cancellationToken = default);

    /// <summary>
    /// True when the doctor has a live (scheduled or checked-in) appointment that overlaps the given window.
    /// </summary>
    Task<bool> HasOverlapAsync(
        Guid doctorId,
        DateTimeOffset windowStart,
        DateTimeOffset windowEnd,
        CancellationToken cancellationToken = default);

    void Add(Appointment appointment);
}
