using Microsoft.EntityFrameworkCore;
using PatientAppointment.Application.Abstractions;
using PatientAppointment.Domain.Entities;

namespace PatientAppointment.Infrastructure.Persistence.Repositories;

public sealed class PatientRepository : IPatientRepository
{
    private readonly PatientAppointmentDbContext _context;

    public PatientRepository(PatientAppointmentDbContext context)
    {
        _context = context;
    }

    public Task<Patient?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _context.Patients.FirstOrDefaultAsync(patient => patient.Id == id, cancellationToken);
    }

    public void Add(Patient patient)
    {
        _context.Patients.Add(patient);
    }
}
