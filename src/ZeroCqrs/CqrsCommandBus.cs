using System;
using System.Threading;
using System.Threading.Tasks;

namespace ZeroCqrs;

public sealed class CqrsZeroCommandBus(IServiceProvider provider) : IZeroCommandBus
{
    public Task Send(IZeroCommand command, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        return CommandDispatcher.For(command.GetType()).Dispatch(command, provider, ct);
    }

    public Task<TResponse> Send<TResponse>(IZeroCommand<TResponse> command, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        return CommandDispatcher<TResponse>.For(command.GetType()).Dispatch(command, provider, ct);
    }
}
