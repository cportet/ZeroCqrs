using System;
using System.Threading;
using System.Threading.Tasks;

namespace ZeroCqrs;

public interface IZeroCommandHandler<in TCommand> where TCommand : IZeroCommand
{
    Task Execute(TCommand command, CancellationToken ct = default);
}

public interface IZeroCommandHandler<in TCommand, TResponse> where TCommand : IZeroCommand<TResponse>
{
    Task<TResponse> Execute(TCommand command, CancellationToken ct = default);
}

/// <summary>
/// Former name of <see cref="IZeroCommandHandler{TCommand, TResponse}"/>, kept so existing handlers
/// keep compiling and being discovered. It will be removed in a future major version.
/// </summary>
[Obsolete("Use IZeroCommandHandler<TCommand, TResponse> instead. ICommandHandler<,> will be removed in a future major version.")]
public interface ICommandHandler<in TCommand, TResponse> : IZeroCommandHandler<TCommand, TResponse>
    where TCommand : IZeroCommand<TResponse>;
