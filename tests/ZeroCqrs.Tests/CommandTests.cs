using Microsoft.Extensions.DependencyInjection;

namespace ZeroCqrs.Tests;

public class CommandTests
{
    private readonly HandlerSpy _spy = new();

    [Fact]
    public async Task ShouldRespectCancellationToken()
    {
        var bus = RegisterCommandBus();

        using var cts = new CancellationTokenSource();
        var task = bus.Send(new DoCancellableCommand(), cts.Token);

        cts.CancelAfter(10);

        await Assert.ThrowsAsync<TaskCanceledException>(() => task);

        Assert.False(_spy.Executed);
    }

    [Fact]
    public async Task ShouldResolveAndExecuteAsyncCommandHandler()
    {
        var bus = RegisterCommandBus();

        await bus.Send(new DoAsyncCommand());

        Assert.True(_spy.Executed);
    }


    [Fact]
    public async Task ShouldResolveAndExecuteCommand()
    {
        var bus = RegisterCommandBus();

        await bus.Send(new DoSomethingCommand());

        Assert.True(_spy.Executed);
    }

    [Fact]
    public async Task ShouldResolveAndFailsIfCommandIsNotHandled()
    {
        var bus = RegisterCommandBus();

        var ex = await Assert.ThrowsAsync<CommandHandlerNotFoundException>(
            () => bus.Send(new DoOtherCommandNotHandled()));

        Assert.Contains(nameof(DoOtherCommandNotHandled), ex.Message);
    }

    [Fact]
    public async Task ShouldResolveAndFailsIfCommandIsNull()
    {
        var bus = RegisterCommandBus();

        var ex = await Assert.ThrowsAsync<ArgumentNullException>(
            () => bus.Send(command: null));

        Assert.Equal("command", ex.ParamName);
    }

    private IZeroCommandBus RegisterCommandBus()
    {
        var services = new ServiceCollection();

        services.AddSingleton(_spy);
        services.AddZeroCqrsCommands(typeof(DoSomethingHandler));

        var provider = services.BuildServiceProvider();
        var bus = provider.GetRequiredService<IZeroCommandBus>();

        return bus;
    }
}

public sealed record DoSomethingCommand : IZeroCommand;

public sealed record DoOtherCommandNotHandled : IZeroCommand;

public sealed record DoAsyncCommand : IZeroCommand;

public sealed record DoCancellableCommand : IZeroCommand;

public class DoSomethingHandler(HandlerSpy spy) :
    IZeroCommandHandler<DoSomethingCommand>,
    IZeroCommandHandler<DoAsyncCommand>,
    IZeroCommandHandler<DoCancellableCommand>
{
    public Task Execute(DoSomethingCommand command, CancellationToken ct = default)
    {
        spy.MarkAsExecuted();
        return Task.CompletedTask;
    }

    public async Task Execute(DoAsyncCommand command, CancellationToken ct = default)
    {
        await Task.Delay(1, ct);
        spy.MarkAsExecuted();
    }

    public async Task Execute(DoCancellableCommand command, CancellationToken ct = default)
    {
        await Task.Delay(10000, ct);
        spy.MarkAsExecuted();
    }
}