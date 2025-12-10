using System.Threading;
using System.Threading.Tasks;

namespace ZeroCqrs;

public interface IQueryHandler<in TQuery, TResponse> where TQuery : IQuery<TResponse>
{
    Task<TResponse> Answer(TQuery query, CancellationToken ct = default);
}