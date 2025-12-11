using Microsoft.Extensions.DependencyInjection;

namespace ZeroCqrs.Tests;

public class CommandWithResponseTests
{
    private readonly HandlerSpy _spy = new();

    [Fact]
    public async Task ShouldRespectCancellationToken()
    {
        var bus = RegisterCommandBus();

        using var cts = new CancellationTokenSource();
        var task = bus.Send(new DoCancellableCommandWithResult(), cts.Token);

        cts.CancelAfter(10);

        await Assert.ThrowsAsync<TaskCanceledException>(() => task);

        Assert.False(_spy.Executed);
    }

    [Fact]
    public async Task ShouldResolveAndExecuteAsyncCommandHandler()
    {
        var bus = RegisterCommandBus();

        var result = await bus.Send(new DoAsyncCommandWithResult());

        var expected = new CommandWithResultResponse("test");

        Assert.True(_spy.Executed);
        Assert.Equal(result, expected);
    }

    [Fact]
    public async Task ShouldResolveAndExecuteCommandWithResponse()
    {
        var bus = RegisterCommandBus();

        var result = await bus.Send(new DoSomethingCommandWithResult());

        var expected = new CommandWithResultResponse("test");

        Assert.True(_spy.Executed);
        Assert.Equal(result, expected);
    }

    [Fact]
    public async Task ShouldResolveAndFailsIfCommandIsNotHandled()
    {
        var bus = RegisterCommandBus();

        var ex = await Assert.ThrowsAsync<CommandHandlerNotFoundException>(
            () => bus.Send(new DoCommandWithResultNotHandled()));

        Assert.Contains(nameof(DoCommandWithResultNotHandled), ex.Message);
        Assert.False(_spy.Executed);
    }

    [Fact]
    public async Task ShouldResolveAndFailsIfCommandIsNull()
    {
        var bus = RegisterCommandBus();

        var ex = await Assert.ThrowsAsync<ArgumentNullException>(
            () => bus.Send<string>(command: null));

        Assert.Equal("command", ex.ParamName);
        Assert.False(_spy.Executed);
    }

    private IZeroCommandBus RegisterCommandBus()
    {
        var services = new ServiceCollection();
        services.AddSingleton(_spy);
        services.AddZeroCqrsCommands(typeof(DoSomethingWithResultHandler));

        var provider = services.BuildServiceProvider();
        var bus = provider.GetRequiredService<IZeroCommandBus>();

        return bus;
    }
}

public sealed record DoSomethingCommandWithResult : IZeroCommand<CommandWithResultResponse>;

public sealed record DoAsyncCommandWithResult : IZeroCommand<CommandWithResultResponse>;

public sealed record DoCancellableCommandWithResult : IZeroCommand<CommandWithResultResponse>;

public sealed class DoCommandWithResultNotHandled : IZeroCommand<bool>;

public sealed record CommandWithResultResponse(string Message);

public class DoSomethingWithResultHandler(HandlerSpy spy) :
    ICommandHandler<DoSomethingCommandWithResult, CommandWithResultResponse>,
    ICommandHandler<DoAsyncCommandWithResult, CommandWithResultResponse>,
    ICommandHandler<DoCancellableCommandWithResult, CommandWithResultResponse>
{

    public Task<CommandWithResultResponse> Execute(
        DoSomethingCommandWithResult command, CancellationToken ct = default)
    {
        spy.MarkAsExecuted();
        return Task.FromResult(new CommandWithResultResponse("test"));
    }

    public async Task<CommandWithResultResponse> Execute(DoCancellableCommandWithResult command, CancellationToken ct = default)
    {
        await Task.Delay(10000, ct);
        spy.MarkAsExecuted();
        return new CommandWithResultResponse("test");
    }

    public async Task<CommandWithResultResponse> Execute(DoAsyncCommandWithResult command, CancellationToken ct = default)
    {
        await Task.Delay(1, ct);
        spy.MarkAsExecuted();
        return new CommandWithResultResponse("test");
    }
}