using System;
using System.Threading;
using System.Threading.Tasks;

namespace ZeroCqrs;

public sealed class CqrsZeroQueryBus(IServiceProvider provider) : IZeroQueryBus
{
    public Task<TResponse> Ask<TResponse>(IZeroQuery<TResponse> query, CancellationToken ct = default)
    {

        ArgumentNullException.ThrowIfNull(query);

        var queryType = query.GetType();
        var handlerType = typeof(IZeroQueryHandler<,>).MakeGenericType(queryType, typeof(TResponse));

        var handler = (dynamic)provider.GetService(handlerType)
                      ?? throw new QueryHandlerNotFoundException(queryType);

        return handler.Answer((dynamic)query, ct);
    }
}