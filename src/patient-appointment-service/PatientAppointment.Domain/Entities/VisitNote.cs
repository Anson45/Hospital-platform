namespace PatientAppointment.Domain.Entities;

/// <summary>
/// A clinical note written by a doctor during a visit. The AI service may summarize it later, but only as a draft.
/// </summary>
public class VisitNote
{
    // EF Core materializes rows through this constructor, so it skips the invariant checks.
    private VisitNote()
    {
        Content = null!;
    }

    public VisitNote(Guid visitId, Guid doctorId, string content, DateTimeOffset createdAt)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(content);

        Id = Guid.NewGuid();
        VisitId = visitId;
        DoctorId = doctorId;
        Content = content;
        CreatedAt = createdAt;
    }

    public Guid Id { get; }

    public Guid VisitId { get; }

    public Guid DoctorId { get; }

    public string Content { get; }

    public DateTimeOffset CreatedAt { get; }
}
