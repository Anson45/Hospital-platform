namespace PatientAppointment.Application.Exceptions;

/// <summary>
/// Raised when a use case refers to an entity that does not exist.
/// </summary>
public sealed class NotFoundException : Exception
{
    public NotFoundException(string entityName, Guid id) : base($"{entityName} '{id}' was not found.")
    {
    }
}
