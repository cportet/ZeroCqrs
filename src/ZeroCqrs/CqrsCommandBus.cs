using System;
using System.Threading;
using System.Threading.Tasks;

namespace ZeroCqrs;

public sealed class CqrsCommandBus(IServiceProvider provider)
{
    public Task Send(ICommand command, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var commandType = command.GetType();
        var handlerType = typeof(ICommandHandler<>).MakeGenericType(commandType);

        var handler = (dynamic)provider.GetService(handlerType)
                      ?? throw new CommandHandlerNotFoundException(commandType);

        return handler.Execute((dynamic)command, ct);
    }

    public Task<TResponse> Send<TResponse>(ICommand<TResponse> command, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var commandType = command.GetType();
        var handlerType = typeof(ICommandHandler<,>).MakeGenericType(commandType, typeof(TResponse));

        var handler = (dynamic)provider.GetService(handlerType)
                      ?? throw new CommandHandlerNotFoundException(commandType);

        return handler.Execute((dynamic)command, ct);
    }
}