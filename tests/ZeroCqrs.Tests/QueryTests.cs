using Microsoft.Extensions.DependencyInjection;

namespace ZeroCqrs.Tests;

public class QueryTests
{
    private readonly HandlerSpy _spy = new();

    [Fact]
    public async Task ShouldRespectCancellationToken()
    {
        var bus = RegisterQueryBus();

        using var cts = new CancellationTokenSource();
        var task = bus.Ask(new PingCancellableQuery(), cts.Token);

        cts.CancelAfter(10);

        await Assert.ThrowsAsync<TaskCanceledException>(() => task);
        Assert.False(_spy.Executed);
    }

    [Fact]
    public async Task ShouldResolveAndExecuteAsyncQueryHandler()
    {
        var bus = RegisterQueryBus();

        var result = await bus.Ask(new PingAsyncQuery());

        Assert.Equal("Pong", result);
        Assert.True(_spy.Executed);
    }

    [Fact]
    public async Task ShouldResolveAndExecuteQueryHandler()
    {
        var bus = RegisterQueryBus();

        var result = await bus.Ask(new PingQuery());

        Assert.Equal("Pong", result);
        Assert.True(_spy.Executed);
    }

    [Fact]
    public async Task ShouldResolveAndFailsIfQueryIsNotHandled()
    {
        var bus = RegisterQueryBus();

        var ex = await Assert.ThrowsAsync<QueryHandlerNotFoundException>(
            () => bus.Ask(new NotHandlerQuery()));

        Assert.Contains(nameof(NotHandlerQuery), ex.Message);
        Assert.False(_spy.Executed);
    }

    [Fact]
    public async Task ShouldResolveAndFailsIfQueryIsNull()
    {
        var bus = RegisterQueryBus();

        var ex = await Assert.ThrowsAsync<ArgumentNullException>(
            () => bus.Ask<string>(query: null));

        Assert.Equal("query", ex.ParamName);
        Assert.False(_spy.Executed);
    }

    private IZeroQueryBus RegisterQueryBus()
    {
        var services = new ServiceCollection();
        services.AddSingleton(_spy);
        services.AddZeroCqrs(typeof(PingQueryHandler));

        var provider = services.BuildServiceProvider();
        var bus = provider.GetRequiredService<IZeroQueryBus>();

        return bus;
    }
}

public record PingQuery : IZeroQuery<string>;

public record PingAsyncQuery : IZeroQuery<string>;

public record PingCancellableQuery : IZeroQuery<string>;

public record NotHandlerQuery : IZeroQuery<string>;

public class PingQueryHandler(HandlerSpy spy) :
    IZeroQueryHandler<PingQuery, string>,
    IZeroQueryHandler<PingAsyncQuery, string>,
    IZeroQueryHandler<PingCancellableQuery, string>
{
    public Task<string> Answer(PingQuery query, CancellationToken ct = default)
    {
        spy.MarkAsExecuted();
        return Task.FromResult("Pong");
    }

    public async Task<string> Answer(PingAsyncQuery query, CancellationToken ct = default)
    {
        await Task.Delay(1, ct);
        spy.MarkAsExecuted();
        return "Pong";
    }

    public async Task<string> Answer(PingCancellableQuery query, CancellationToken ct = default)
    {
        await Task.Delay(10000, ct);
        spy.MarkAsExecuted();
        return "Pong";
    }
}