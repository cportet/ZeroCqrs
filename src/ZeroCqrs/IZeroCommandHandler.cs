using System;
using System.Threading;
using System.Threading.Tasks;

namespace ZeroCqrs;

/// <summary>
/// Executes a command without result. Exactly one handler must exist per command type.
/// </summary>
/// <typeparam name="TCommand">The command type.</typeparam>
public interface IZeroCommandHandler<in TCommand> where TCommand : IZeroCommand
{
    /// <summary>
    /// Executes <paramref name="command"/>.
    /// </summary>
    /// <param name="command">The command to execute.</param>
    /// <param name="ct">Token to observe for cancellation.</param>
    Task Execute(TCommand command, CancellationToken ct = default);
}

/// <summary>
/// Executes a command and returns its result. Exactly one handler must exist per command type.
/// </summary>
/// <typeparam name="TCommand">The command type.</typeparam>
/// <typeparam name="TResponse">Type of the result.</typeparam>
public interface IZeroCommandHandler<in TCommand, TResponse> where TCommand : IZeroCommand<TResponse>
{
    /// <summary>
    /// Executes <paramref name="command"/>.
    /// </summary>
    /// <param name="command">The command to execute.</param>
    /// <param name="ct">Token to observe for cancellation.</param>
    /// <returns>The command result.</returns>
    Task<TResponse> Execute(TCommand command, CancellationToken ct = default);
}

/// <summary>
/// Former name of <see cref="IZeroCommandHandler{TCommand, TResponse}"/>, kept so existing handlers
/// keep compiling and being discovered. It will be removed in a future major version.
/// </summary>
/// <typeparam name="TCommand">The command type.</typeparam>
/// <typeparam name="TResponse">Type of the result.</typeparam>
[Obsolete("Use IZeroCommandHandler<TCommand, TResponse> instead. ICommandHandler<,> will be removed in a future major version.")]
public interface ICommandHandler<in TCommand, TResponse> : IZeroCommandHandler<TCommand, TResponse>
    where TCommand : IZeroCommand<TResponse>;
