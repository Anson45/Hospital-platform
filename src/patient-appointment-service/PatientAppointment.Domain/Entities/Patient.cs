namespace PatientAppointment.Domain.Entities;

/// <summary>
/// A person receiving care. This study project uses synthetic data only.
/// </summary>
public class Patient
{
    // EF Core materializes rows through this constructor, so it skips the invariant checks.
    private Patient()
    {
        FirstName = null!;
        LastName = null!;
    }

    public Patient(
        string firstName,
        string lastName,
        DateOnly dateOfBirth,
        DateTimeOffset registeredAt,
        string? email = null,
        string? phoneNumber = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(firstName);
        ArgumentException.ThrowIfNullOrWhiteSpace(lastName);

        Id = Guid.NewGuid();
        FirstName = firstName;
        LastName = lastName;
        DateOfBirth = dateOfBirth;
        RegisteredAt = registeredAt;
        Email = email;
        PhoneNumber = phoneNumber;
    }

    public Guid Id { get; }

    public string FirstName { get; private set; }

    public string LastName { get; private set; }

    public DateOnly DateOfBirth { get; }

    public string? Email { get; private set; }

    public string? PhoneNumber { get; private set; }

    public string? MedicalHistorySummary { get; private set; }

    public DateTimeOffset RegisteredAt { get; }

    /// <summary>
    /// Replaces the contact details. Null clears a value.
    /// </summary>
    public void UpdateContact(string? email, string? phoneNumber)
    {
        Email = email;
        PhoneNumber = phoneNumber;
    }

    public void UpdateMedicalHistorySummary(string? summary)
    {
        MedicalHistorySummary = summary;
    }
}
