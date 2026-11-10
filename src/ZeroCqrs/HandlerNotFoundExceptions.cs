using System;

namespace ZeroCqrs;

/// <summary>
/// Thrown by <see cref="IZeroCommandBus"/> when no handler is registered for a command type.
/// </summary>
/// <param name="messageType">The command type without handler.</param>
public sealed class CommandHandlerNotFoundException(Type messageType) : InvalidOperationException(
    $"No handler registered for command '{messageType.FullName}'. " +
    "Implement IZeroCommandHandler<TCommand> (or IZeroCommandHandler<TCommand, TResponse> for a command " +
    "with a result) and check that its assembly is scanned by AddZeroCqrs / AddZeroCqrsCommands.")
{
    /// <summary>The command type without handler.</summary>
    public Type MessageType { get; } = messageType;
}

/// <summary>
/// Thrown by <see cref="IZeroQueryBus"/> when no handler is registered for a query type.
/// </summary>
/// <param name="messageType">The query type without handler.</param>
public sealed class QueryHandlerNotFoundException(Type messageType) : InvalidOperationException(
    $"No handler registered for query '{messageType.FullName}'. " +
    "Implement IZeroQueryHandler<TQuery, TResponse> and check that its assembly is scanned " +
    "by AddZeroCqrs / AddZeroCqrsQueries.")
{
    /// <summary>The query type without handler.</summary>
    public Type MessageType { get; } = messageType;
}
