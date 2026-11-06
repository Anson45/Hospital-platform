using PatientAppointment.Domain.Exceptions;

namespace PatientAppointment.Domain.Entities;

/// <summary>
/// The actual encounter, opened when a patient checks in for an appointment and closed when it ends.
/// </summary>
public class Visit
{
    // EF Core materializes rows through this constructor.
    private Visit()
    {
    }

    public Visit(Guid appointmentId, Guid patientId, DateTimeOffset checkedInAt)
    {
        Id = Guid.NewGuid();
        AppointmentId = appointmentId;
        PatientId = patientId;
        CheckedInAt = checkedInAt;
    }

    public Guid Id { get; }

    public Guid AppointmentId { get; }

    public Guid PatientId { get; }

    public DateTimeOffset CheckedInAt { get; }

    public DateTimeOffset? EndedAt { get; private set; }

    public bool IsOpen => EndedAt is null;

    public void End(DateTimeOffset endedAt)
    {
        if (!IsOpen)
        {
            throw new DomainException("The visit has already ended.");
        }

        if (endedAt < CheckedInAt)
        {
            throw new DomainException("A visit cannot end before it was checked in.");
        }

        EndedAt = endedAt;
    }
}
