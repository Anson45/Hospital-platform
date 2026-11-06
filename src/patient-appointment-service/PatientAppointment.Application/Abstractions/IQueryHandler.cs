namespace PatientAppointment.Application.Abstractions;

/// <summary>
/// Handles a read use case. Queries never change state.
/// </summary>
public interface IQueryHandler<in TQuery, TResult>
{
    Task<TResult> HandleAsync(TQuery query, CancellationToken cancellationToken = default);
}
