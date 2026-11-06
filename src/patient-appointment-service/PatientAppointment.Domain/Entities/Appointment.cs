using PatientAppointment.Domain.Enums;
using PatientAppointment.Domain.Exceptions;

namespace PatientAppointment.Domain.Entities;

/// <summary>
/// A booked slot between a patient and a doctor. Status changes follow a fixed lifecycle:
/// Scheduled leads to CheckedIn, Cancelled, or NoShow; CheckedIn leads to Completed.
/// </summary>
public class Appointment
{
    // EF Core materializes rows through this constructor.
    private Appointment()
    {
    }

    public Appointment(Guid patientId, Guid doctorId, DateTimeOffset scheduledStart, TimeSpan duration)
    {
        if (duration <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(duration), "Duration must be positive.");
        }

        Id = Guid.NewGuid();
        PatientId = patientId;
        DoctorId = doctorId;
        Duration = duration;
        ScheduledStart = scheduledStart;
        ScheduledEnd = scheduledStart + duration;
        Status = AppointmentStatus.Scheduled;
    }

    public Guid Id { get; }

    public Guid PatientId { get; }

    public Guid DoctorId { get; }

    public DateTimeOffset ScheduledStart { get; private set; }

    /// <summary>
    /// Stored rather than computed so overlap checks can run as a plain SQL comparison.
    /// </summary>
    public DateTimeOffset ScheduledEnd { get; private set; }

    public TimeSpan Duration { get; }

    public AppointmentStatus Status { get; private set; }

    public void Reschedule(DateTimeOffset newStart)
    {
        EnsureStatus(AppointmentStatus.Scheduled);
        ScheduledStart = newStart;
        ScheduledEnd = newStart + Duration;
    }

    public void Cancel()
    {
        EnsureStatus(AppointmentStatus.Scheduled);
        Status = AppointmentStatus.Cancelled;
    }

    public void MarkNoShow()
    {
        EnsureStatus(AppointmentStatus.Scheduled);
        Status = AppointmentStatus.NoShow;
    }

    public void CheckIn()
    {
        EnsureStatus(AppointmentStatus.Scheduled);
        Status = AppointmentStatus.CheckedIn;
    }

    public void Complete()
    {
        EnsureStatus(AppointmentStatus.CheckedIn);
        Status = AppointmentStatus.Completed;
    }

    private void EnsureStatus(AppointmentStatus expected)
    {
        if (Status != expected)
        {
            throw new DomainException($"Expected appointment status {expected}, but it is {Status}.");
        }
    }
}
