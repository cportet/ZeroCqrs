using Microsoft.Extensions.DependencyInjection;

namespace ZeroCqrs.Tests;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddZeroCqrsWithoutParametersShouldScanCallingAssembly()
    {
        var services = new ServiceCollection();

        services.AddZeroCqrs();

        var provider = services.BuildServiceProvider();
        var queryBus = provider.GetService<IZeroQueryBus>();
        var commandBus = provider.GetService<IZeroCommandBus>();

        Assert.NotNull(queryBus);
        Assert.NotNull(commandBus);
    }

    [Fact]
    public void AddZeroCqrsWithMarkerTypeShouldScanOnlyThatAssembly()
    {
        var services = new ServiceCollection();

        services.AddZeroCqrs(typeof(ServiceCollectionExtensionsTests));

        var provider = services.BuildServiceProvider();
        Assert.NotNull(provider.GetService<IZeroQueryBus>());
        Assert.NotNull(provider.GetService<IZeroCommandBus>());
    }

    [Fact]
    public void AddZeroCqrsWithoutTypeShouldScanEntryAssembly()
    {
        var services = new ServiceCollection();

        services.AddZeroCqrs();

        var provider = services.BuildServiceProvider();
        Assert.NotNull(provider.GetService<IZeroQueryBus>());
        Assert.NotNull(provider.GetService<IZeroCommandBus>());
    }

    [Fact]
    public void AddZeroCqrsWithMultipleMarkersShouldScanDistinctAssemblies()
    {
        var services = new ServiceCollection();

        services.AddZeroCqrs(
            typeof(ServiceCollectionExtensionsTests),
            typeof(ServiceCollection)
        );

        var provider = services.BuildServiceProvider();
        Assert.NotNull(provider.GetService<IZeroQueryBus>());
        Assert.NotNull(provider.GetService<IZeroCommandBus>());
    }

    [Fact]
    public async Task AddZeroCqrsWithReferenceToHandlerShouldWork()
    {
        var services = new ServiceCollection();

        services.AddZeroCqrsQueries(typeof(DummyQueryHandler));
        services.AddZeroCqrsCommands(typeof(DummyCommandHandler));

        var provider = services.BuildServiceProvider();
        var queryBus = provider.GetRequiredService<IZeroQueryBus>();
        var commandBus = provider.GetRequiredService<IZeroCommandBus>();

        var queryResult = await queryBus.Ask(new DummyQuery());
        Assert.Equal("OK", queryResult);

        var commandResult = await commandBus.Send(new DummyCommand());
        Assert.Equal("OK", commandResult);
    }

    [Fact]
    public async Task AddZeroCqrsWithReferenceToCommandHandlerShouldFailOnQuery()
    {
        var services = new ServiceCollection();

        services.AddZeroCqrsCommands(typeof(DummyCommandHandler));

        var provider = services.BuildServiceProvider();
        Assert.Null(provider.GetService<IZeroQueryBus>());
        Assert.NotNull(provider.GetService<IZeroCommandBus>());

        var commandBus = provider.GetRequiredService<IZeroCommandBus>();

        var commandResult = await commandBus.Send(new DummyCommand());
        Assert.Equal("OK", commandResult);
    }

    [Fact]
    public async Task AddZeroCqrsWithReferenceToQueryHandlerShouldFailOnCommand()
    {
        var services = new ServiceCollection();

        services.AddZeroCqrsQueries(typeof(DummyQueryHandler));

        var provider = services.BuildServiceProvider();
        Assert.NotNull(provider.GetService<IZeroQueryBus>());
        Assert.Null(provider.GetService<IZeroCommandBus>());

        var queryBus = provider.GetRequiredService<IZeroQueryBus>();

        var queryResult = await queryBus.Ask(new DummyQuery());
        Assert.Equal("OK", queryResult);
    }
}

public sealed record DummyQuery : IZeroQuery<string>;

public sealed record DummyCommand : IZeroCommand<string>;

public class DummyQueryHandler : IZeroQueryHandler<DummyQuery, string>
{
    public Task<string> Answer(DummyQuery query, CancellationToken ct = default) => Task.FromResult("OK");
}

public class DummyCommandHandler : ICommandHandler<DummyCommand, string>
{
    public Task<string> Execute(DummyCommand command, CancellationToken ct = default) => Task.FromResult("OK");
}