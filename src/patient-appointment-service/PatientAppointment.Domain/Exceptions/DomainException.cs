namespace PatientAppointment.Domain.Exceptions;

/// <summary>
/// Signals a violated business rule. Entities throw this so invalid state never leaves the Domain layer.
/// </summary>
public sealed class DomainException : Exception
{
    public DomainException(string message) : base(message)
    {
    }
}
