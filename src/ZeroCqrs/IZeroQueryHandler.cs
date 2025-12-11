using System.Threading;
using System.Threading.Tasks;

namespace ZeroCqrs;

public interface IZeroQueryHandler<in TQuery, TResponse> where TQuery : IZeroQuery<TResponse>
{
    Task<TResponse> Answer(TQuery query, CancellationToken ct = default);
}