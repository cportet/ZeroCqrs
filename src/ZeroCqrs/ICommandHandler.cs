using System.Threading;
using System.Threading.Tasks;

namespace ZeroCqrs;

public interface ICommandHandler<in TCommand> where TCommand : ICommand
{
    Task Execute(TCommand command, CancellationToken ct = default);
}

public interface ICommandHandler<in TCommand, TResponse> where TCommand : ICommand<TResponse>
{
    Task<TResponse> Execute(TCommand command, CancellationToken ct = default);
}