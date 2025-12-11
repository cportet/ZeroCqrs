using System.Threading;
using System.Threading.Tasks;

namespace ZeroCqrs;

public interface IZeroCommandHandler<in TCommand> where TCommand : IZeroCommand
{
    Task Execute(TCommand command, CancellationToken ct = default);
}

public interface ICommandHandler<in TCommand, TResponse> where TCommand : IZeroCommand<TResponse>
{
    Task<TResponse> Execute(TCommand command, CancellationToken ct = default);
}