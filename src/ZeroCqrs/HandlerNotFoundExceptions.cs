using System;

namespace ZeroCqrs;

public sealed class CommandHandlerNotFoundException(Type messageType) : InvalidOperationException(
    $"No handler registered for message type '{messageType.FullName}'. " +
    $"Did you forget to implement ICommandHandler<,> for this type?")
{
    public Type MessageType { get; } = messageType;
}


public sealed class QueryHandlerNotFoundException(Type messageType) : InvalidOperationException(
    $"No handler registered for message type '{messageType.FullName}'. " +
    $"Did you forget to implement IQueryHandler<,> for this type?")
{
    public Type MessageType { get; } = messageType;
}