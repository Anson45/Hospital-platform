using PatientAppointment.Domain.Entities;

namespace PatientAppointment.Application.Abstractions;

public interface IDepartmentRepository
{
    Task<Department?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    void Add(Department department);
}
