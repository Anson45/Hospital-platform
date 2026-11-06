using PatientAppointment.Domain.Entities;

namespace PatientAppointment.Application.Abstractions;

public interface IVisitRepository
{
    Task<Visit?> GetOpenByAppointmentIdAsync(Guid appointmentId, CancellationToken cancellationToken = default);

    void Add(Visit visit);
}
