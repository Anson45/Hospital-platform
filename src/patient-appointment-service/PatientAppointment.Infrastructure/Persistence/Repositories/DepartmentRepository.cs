using Microsoft.EntityFrameworkCore;
using PatientAppointment.Application.Abstractions;
using PatientAppointment.Domain.Entities;

namespace PatientAppointment.Infrastructure.Persistence.Repositories;

public sealed class DepartmentRepository : IDepartmentRepository
{
    private readonly PatientAppointmentDbContext _context;

    public DepartmentRepository(PatientAppointmentDbContext context)
    {
        _context = context;
    }

    public Task<Department?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _context.Departments.FirstOrDefaultAsync(department => department.Id == id, cancellationToken);
    }

    public void Add(Department department)
    {
        _context.Departments.Add(department);
    }
}
