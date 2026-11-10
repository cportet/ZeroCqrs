using System;
using System.Threading;
using System.Threading.Tasks;

namespace ZeroCqrs;

public sealed class CqrsZeroQueryBus(IServiceProvider provider) : IZeroQueryBus
{
    public Task<TResponse> Ask<TResponse>(IZeroQuery<TResponse> query, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        return QueryDispatcher<TResponse>.For(query.GetType()).Dispatch(query, provider, ct);
    }
}
