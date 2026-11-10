using Microsoft.Extensions.DependencyInjection;

namespace ZeroCqrs.Tests;

public class DispatchTests
{
    private static IServiceProvider BuildProvider()
    {
        var services = new ServiceCollection();
        services.AddZeroCqrs(typeof(DispatchTests));
        return services.BuildServiceProvider();
    }

    [Fact]
    public async Task QueryShouldDispatchToInternalHandler()
    {
        var bus = BuildProvider().GetRequiredService<IZeroQueryBus>();
        Assert.Equal(1, await bus.Ask(new QueryWithInternalHandler()));
    }

    [Fact]
    public async Task QueryShouldDispatchInternalMessage()
    {
        var bus = BuildProvider().GetRequiredService<IZeroQueryBus>();
        Assert.Equal(2, await bus.Ask(new InternalQuery()));
    }

    [Fact]
    public async Task QueryShouldDispatchToExplicitImplementation()
    {
        var bus = BuildProvider().GetRequiredService<IZeroQueryBus>();
        Assert.Equal(3, await bus.Ask(new QueryWithExplicitHandler()));
    }

    [Fact]
    public async Task CommandShouldDispatchToInternalExplicitHandler()
    {
        var bus = BuildProvider().GetRequiredService<IZeroCommandBus>();
        var command = new InternalCommand();

        await bus.Send(command);

        Assert.True(command.Executed);
    }

    [Fact]
    public async Task CommandWithResponseShouldDispatchToInternalExplicitHandler()
    {
        var bus = BuildProvider().GetRequiredService<IZeroCommandBus>();
        Assert.Equal(4, await bus.Send(new InternalCommandWithResponse()));
    }

    [Fact]
    public async Task DispatchShouldWorkOnRepeatedCalls()
    {
        var bus = BuildProvider().GetRequiredService<IZeroQueryBus>();
        Assert.Equal(1, await bus.Ask(new QueryWithInternalHandler()));
        Assert.Equal(1, await bus.Ask(new QueryWithInternalHandler()));
    }
}

public sealed record QueryWithInternalHandler : IZeroQuery<int>;

internal sealed class InternalQueryHandler : IZeroQueryHandler<QueryWithInternalHandler, int>
{
    public Task<int> Answer(QueryWithInternalHandler query, CancellationToken ct = default) => Task.FromResult(1);
}

internal sealed record InternalQuery : IZeroQuery<int>;

internal sealed class InternalQueryMessageHandler : IZeroQueryHandler<InternalQuery, int>
{
    public Task<int> Answer(InternalQuery query, CancellationToken ct = default) => Task.FromResult(2);
}

public sealed record QueryWithExplicitHandler : IZeroQuery<int>;

public sealed class ExplicitQueryHandler : IZeroQueryHandler<QueryWithExplicitHandler, int>
{
    Task<int> IZeroQueryHandler<QueryWithExplicitHandler, int>.Answer(QueryWithExplicitHandler query, CancellationToken ct)
        => Task.FromResult(3);
}

internal sealed class InternalCommand : IZeroCommand
{
    public bool Executed { get; set; }
}

internal sealed record InternalCommandWithResponse : IZeroCommand<int>;

internal sealed class InternalExplicitCommandHandler :
    IZeroCommandHandler<InternalCommand>,
    IZeroCommandHandler<InternalCommandWithResponse, int>
{
    Task IZeroCommandHandler<InternalCommand>.Execute(InternalCommand command, CancellationToken ct)
    {
        command.Executed = true;
        return Task.CompletedTask;
    }

    Task<int> IZeroCommandHandler<InternalCommandWithResponse, int>.Execute(InternalCommandWithResponse command, CancellationToken ct)
        => Task.FromResult(4);
}
