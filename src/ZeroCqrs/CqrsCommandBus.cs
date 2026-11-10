using System;
using System.Threading;
using System.Threading.Tasks;

namespace ZeroCqrs;

/// <summary>
/// Default <see cref="IZeroCommandBus"/>: resolves the handler from the current service provider.
/// Registered as scoped by <see cref="ServiceCollectionExtensions.AddZeroCqrsCommands"/>.
/// </summary>
/// <param name="provider">Service provider used to resolve the handlers (the current scope).</param>
public sealed class CqrsZeroCommandBus(IServiceProvider provider) : IZeroCommandBus
{
    /// <inheritdoc />
    public Task Send(IZeroCommand command, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        return CommandDispatcher.For(command.GetType()).Dispatch(command, provider, ct);
    }

    /// <inheritdoc />
    public Task<TResponse> Send<TResponse>(IZeroCommand<TResponse> command, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        return CommandDispatcher<TResponse>.For(command.GetType()).Dispatch(command, provider, ct);
    }
}
