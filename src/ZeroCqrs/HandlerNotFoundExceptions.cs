using System;

namespace ZeroCqrs;

public sealed class CommandHandlerNotFoundException(Type messageType) : InvalidOperationException(
    $"No handler registered for command '{messageType.FullName}'. " +
    "Implement IZeroCommandHandler<TCommand> (or IZeroCommandHandler<TCommand, TResponse> for a command " +
    "with a result) and check that its assembly is scanned by AddZeroCqrs / AddZeroCqrsCommands.")
{
    public Type MessageType { get; } = messageType;
}

public sealed class QueryHandlerNotFoundException(Type messageType) : InvalidOperationException(
    $"No handler registered for query '{messageType.FullName}'. " +
    "Implement IZeroQueryHandler<TQuery, TResponse> and check that its assembly is scanned " +
    "by AddZeroCqrs / AddZeroCqrsQueries.")
{
    public Type MessageType { get; } = messageType;
}
