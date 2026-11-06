namespace PatientAppointment.Application.Abstractions;

/// <summary>
/// Handles a write use case that returns a result, such as the id of the entity it created.
/// </summary>
public interface ICommandHandler<in TCommand, TResult>
{
    Task<TResult> HandleAsync(TCommand command, CancellationToken cancellationToken = default);
}

/// <summary>
/// Handles a write use case that returns nothing.
/// </summary>
public interface ICommandHandler<in TCommand>
{
    Task HandleAsync(TCommand command, CancellationToken cancellationToken = default);
}
