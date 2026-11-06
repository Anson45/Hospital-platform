using Microsoft.EntityFrameworkCore;
using PatientAppointment.Application.Abstractions;
using PatientAppointment.Domain.Entities;
using PatientAppointment.Domain.Enums;

namespace PatientAppointment.Infrastructure.Persistence.Repositories;

public sealed class AppointmentRepository : IAppointmentRepository
{
    private readonly PatientAppointmentDbContext _context;

    public AppointmentRepository(PatientAppointmentDbContext context)
    {
        _context = context;
    }

    public Task<Appointment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _context.Appointments.FirstOrDefaultAsync(appointment => appointment.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Appointment>> GetByPatientIdAsync(
        Guid patientId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Appointments
            .Where(appointment => appointment.PatientId == patientId)
            .OrderBy(appointment => appointment.ScheduledStart)
            .ToListAsync(cancellationToken);
    }

    public Task<bool> HasOverlapAsync(
        Guid doctorId,
        DateTimeOffset windowStart,
        DateTimeOffset windowEnd,
        CancellationToken cancellationToken = default)
    {
        // Cancelled and no-show bookings free the slot, so only live statuses count.
        return _context.Appointments.AnyAsync(
            appointment => appointment.DoctorId == doctorId
                && (appointment.Status == AppointmentStatus.Scheduled
                    || appointment.Status == AppointmentStatus.CheckedIn)
                && appointment.ScheduledStart < windowEnd
                && appointment.ScheduledEnd > windowStart,
            cancellationToken);
    }

    public void Add(Appointment appointment)
    {
        _context.Appointments.Add(appointment);
    }
}
