namespace PatientAppointment.Domain.Entities;

/// <summary>
/// A clinical unit (for example, Cardiology) that groups doctors.
/// </summary>
public class Department
{
    // EF Core materializes rows through this constructor, so it skips the invariant checks.
    private Department()
    {
        Name = null!;
    }

    public Department(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        Id = Guid.NewGuid();
        Name = name;
    }

    public Guid Id { get; }

    public string Name { get; private set; }
}
