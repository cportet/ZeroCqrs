using System.Threading;
using System.Threading.Tasks;

namespace ZeroCqrs;

public interface IZeroQueryBus
{
    Task<TResponse> Ask<TResponse>(IZeroQuery<TResponse> query, CancellationToken ct = default);
}

public interface IZeroCommandBus
{
    Task Send(IZeroCommand command, CancellationToken ct = default);
    Task<TResponse> Send<TResponse>(IZeroCommand<TResponse> command, CancellationToken ct = default);
}

public interface IZeroQuery<out TResponse> { }

public interface IZeroCommand { }

public interface IZeroCommand<out TResponse> { }
