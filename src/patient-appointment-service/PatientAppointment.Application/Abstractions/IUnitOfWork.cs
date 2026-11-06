namespace PatientAppointment.Application.Abstractions;

/// <summary>
/// Commits every change tracked for one use case in a single transaction.
/// </summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
