using Microsoft.EntityFrameworkCore;
using PatientAppointment.Application.Abstractions;
using PatientAppointment.Domain.Entities;

namespace PatientAppointment.Infrastructure.Persistence.Repositories;

public sealed class DoctorRepository : IDoctorRepository
{
    private readonly PatientAppointmentDbContext _context;

    public DoctorRepository(PatientAppointmentDbContext context)
    {
        _context = context;
    }

    public Task<Doctor?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _context.Doctors
            .Include(doctor => doctor.Schedules)
            .FirstOrDefaultAsync(doctor => doctor.Id == id, cancellationToken);
    }

    public void Add(Doctor doctor)
    {
        _context.Doctors.Add(doctor);
    }
}
