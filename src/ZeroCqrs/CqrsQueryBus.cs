using System;
using System.Threading;
using System.Threading.Tasks;

namespace ZeroCqrs;

/// <summary>
/// Default <see cref="IZeroQueryBus"/>: resolves the handler from the current service provider.
/// Registered as scoped by <see cref="ServiceCollectionExtensions.AddZeroCqrsQueries"/>.
/// </summary>
/// <param name="provider">Service provider used to resolve the handlers (the current scope).</param>
public sealed class CqrsZeroQueryBus(IServiceProvider provider) : IZeroQueryBus
{
    /// <inheritdoc />
    public Task<TResponse> Ask<TResponse>(IZeroQuery<TResponse> query, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        return QueryDispatcher<TResponse>.For(query.GetType()).Dispatch(query, provider, ct);
    }
}
