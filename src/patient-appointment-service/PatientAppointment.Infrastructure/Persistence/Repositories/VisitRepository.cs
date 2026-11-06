using Microsoft.EntityFrameworkCore;
using PatientAppointment.Application.Abstractions;
using PatientAppointment.Domain.Entities;

namespace PatientAppointment.Infrastructure.Persistence.Repositories;

public sealed class VisitRepository : IVisitRepository
{
    private readonly PatientAppointmentDbContext _context;

    public VisitRepository(PatientAppointmentDbContext context)
    {
        _context = context;
    }

    public Task<Visit?> GetOpenByAppointmentIdAsync(Guid appointmentId, CancellationToken cancellationToken = default)
    {
        return _context.Visits.FirstOrDefaultAsync(
            visit => visit.AppointmentId == appointmentId && visit.EndedAt == null,
            cancellationToken);
    }

    public void Add(Visit visit)
    {
        _context.Visits.Add(visit);
    }
}
