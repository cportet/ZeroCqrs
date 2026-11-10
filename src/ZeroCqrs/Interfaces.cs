using System.Threading;
using System.Threading.Tasks;

namespace ZeroCqrs;

/// <summary>
/// Sends a query to its single handler and returns the result.
/// </summary>
public interface IZeroQueryBus
{
    /// <summary>
    /// Sends <paramref name="query"/> to its <see cref="IZeroQueryHandler{TQuery, TResponse}"/> and returns its answer.
    /// </summary>
    /// <typeparam name="TResponse">Type of the result returned by the query.</typeparam>
    /// <param name="query">The query to answer.</param>
    /// <param name="ct">Token passed to the handler.</param>
    /// <returns>The result produced by the handler.</returns>
    /// <exception cref="System.ArgumentNullException"><paramref name="query"/> is <see langword="null"/>.</exception>
    /// <exception cref="QueryHandlerNotFoundException">No handler is registered for the query type.</exception>
    Task<TResponse> Ask<TResponse>(IZeroQuery<TResponse> query, CancellationToken ct = default);
}

/// <summary>
/// Sends a command to its single handler.
/// </summary>
public interface IZeroCommandBus
{
    /// <summary>
    /// Sends <paramref name="command"/> to its <see cref="IZeroCommandHandler{TCommand}"/>.
    /// </summary>
    /// <param name="command">The command to execute.</param>
    /// <param name="ct">Token passed to the handler.</param>
    /// <exception cref="System.ArgumentNullException"><paramref name="command"/> is <see langword="null"/>.</exception>
    /// <exception cref="CommandHandlerNotFoundException">No handler is registered for the command type.</exception>
    Task Send(IZeroCommand command, CancellationToken ct = default);

    /// <summary>
    /// Sends <paramref name="command"/> to its <see cref="IZeroCommandHandler{TCommand, TResponse}"/> and returns its result.
    /// </summary>
    /// <typeparam name="TResponse">Type of the result returned by the command.</typeparam>
    /// <param name="command">The command to execute.</param>
    /// <param name="ct">Token passed to the handler.</param>
    /// <returns>The result produced by the handler.</returns>
    /// <exception cref="System.ArgumentNullException"><paramref name="command"/> is <see langword="null"/>.</exception>
    /// <exception cref="CommandHandlerNotFoundException">No handler is registered for the command type.</exception>
    Task<TResponse> Send<TResponse>(IZeroCommand<TResponse> command, CancellationToken ct = default);
}

/// <summary>
/// Marker for a query: a request that reads data and returns a <typeparamref name="TResponse"/>.
/// </summary>
/// <typeparam name="TResponse">Type of the result.</typeparam>
public interface IZeroQuery<out TResponse> { }

/// <summary>
/// Marker for a command without result: a request that performs an action.
/// </summary>
public interface IZeroCommand { }

/// <summary>
/// Marker for a command that performs an action and returns a <typeparamref name="TResponse"/>.
/// </summary>
/// <typeparam name="TResponse">Type of the result.</typeparam>
public interface IZeroCommand<out TResponse> { }
