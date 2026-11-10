using System.Threading;
using System.Threading.Tasks;

namespace ZeroCqrs;

/// <summary>
/// Answers a query. Exactly one handler must exist per query type.
/// </summary>
/// <typeparam name="TQuery">The query type.</typeparam>
/// <typeparam name="TResponse">Type of the result.</typeparam>
public interface IZeroQueryHandler<in TQuery, TResponse> where TQuery : IZeroQuery<TResponse>
{
    /// <summary>
    /// Answers <paramref name="query"/>.
    /// </summary>
    /// <param name="query">The query to answer.</param>
    /// <param name="ct">Token to observe for cancellation.</param>
    /// <returns>The query result.</returns>
    Task<TResponse> Answer(TQuery query, CancellationToken ct = default);
}
