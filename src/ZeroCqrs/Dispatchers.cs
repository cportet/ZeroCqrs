using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace ZeroCqrs;

// Typed dispatchers, built once per message type and cached.
// Handlers are invoked through their interface, so internal handlers/messages
// and explicit interface implementations work (unlike `dynamic` dispatch).

internal abstract class QueryDispatcher<TResponse>
{
    private static readonly ConcurrentDictionary<Type, QueryDispatcher<TResponse>> Cache = new();

    public static QueryDispatcher<TResponse> For(Type queryType) =>
        Cache.GetOrAdd(queryType, static t => (QueryDispatcher<TResponse>)Activator.CreateInstance(
            typeof(QueryDispatcher<,>).MakeGenericType(t, typeof(TResponse)))!);

    public abstract Task<TResponse> Dispatch(IZeroQuery<TResponse> query, IServiceProvider provider, CancellationToken ct);
}

internal sealed class QueryDispatcher<TQuery, TResponse> : QueryDispatcher<TResponse>
    where TQuery : IZeroQuery<TResponse>
{
    public override Task<TResponse> Dispatch(IZeroQuery<TResponse> query, IServiceProvider provider, CancellationToken ct)
    {
        var handler = provider.GetService<IZeroQueryHandler<TQuery, TResponse>>()
                      ?? throw new QueryHandlerNotFoundException(typeof(TQuery));
        return handler.Answer((TQuery)query, ct);
    }
}

internal abstract class CommandDispatcher
{
    private static readonly ConcurrentDictionary<Type, CommandDispatcher> Cache = new();

    public static CommandDispatcher For(Type commandType) =>
        Cache.GetOrAdd(commandType, static t => (CommandDispatcher)Activator.CreateInstance(
            typeof(VoidCommandDispatcher<>).MakeGenericType(t))!);

    public abstract Task Dispatch(IZeroCommand command, IServiceProvider provider, CancellationToken ct);
}

internal sealed class VoidCommandDispatcher<TCommand> : CommandDispatcher
    where TCommand : IZeroCommand
{
    public override Task Dispatch(IZeroCommand command, IServiceProvider provider, CancellationToken ct)
    {
        var handler = provider.GetService<IZeroCommandHandler<TCommand>>()
                      ?? throw new CommandHandlerNotFoundException(typeof(TCommand));
        return handler.Execute((TCommand)command, ct);
    }
}

internal abstract class CommandDispatcher<TResponse>
{
    private static readonly ConcurrentDictionary<Type, CommandDispatcher<TResponse>> Cache = new();

    public static CommandDispatcher<TResponse> For(Type commandType) =>
        Cache.GetOrAdd(commandType, static t => (CommandDispatcher<TResponse>)Activator.CreateInstance(
            typeof(CommandDispatcher<,>).MakeGenericType(t, typeof(TResponse)))!);

    public abstract Task<TResponse> Dispatch(IZeroCommand<TResponse> command, IServiceProvider provider, CancellationToken ct);
}

internal sealed class CommandDispatcher<TCommand, TResponse> : CommandDispatcher<TResponse>
    where TCommand : IZeroCommand<TResponse>
{
    public override Task<TResponse> Dispatch(IZeroCommand<TResponse> command, IServiceProvider provider, CancellationToken ct)
    {
        var handler = provider.GetService<IZeroCommandHandler<TCommand, TResponse>>()
                      ?? throw new CommandHandlerNotFoundException(typeof(TCommand));
        return handler.Execute((TCommand)command, ct);
    }
}
