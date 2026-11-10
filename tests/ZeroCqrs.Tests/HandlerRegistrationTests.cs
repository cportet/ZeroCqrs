using Microsoft.Extensions.DependencyInjection;

namespace ZeroCqrs.Tests;

public class HandlerRegistrationTests
{
    private static bool IsCommandHandler(Type i) =>
        i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IZeroCommandHandler<>);

    [Fact]
    public void ScanShouldIgnoreOpenGenericHandlers()
    {
        var services = new ServiceCollection();

        // The test assembly contains the open generic DuplicateCommandHandler<T>.
        services.AddZeroCqrs(typeof(HandlerRegistrationTests));

        using var provider = services.BuildServiceProvider(validateScopes: true);
        Assert.DoesNotContain(services, d => d.ImplementationType?.IsGenericTypeDefinition == true);
    }

    [Fact]
    public void RegisteringTwiceShouldNotDuplicateBusesOrHandlers()
    {
        var services = new ServiceCollection();

        services.AddZeroCqrs(typeof(HandlerRegistrationTests));
        var count = services.Count;
        services.AddZeroCqrs(typeof(HandlerRegistrationTests));

        Assert.Equal(count, services.Count);
    }

    [Fact]
    public void UserRegistrationShouldBeKept()
    {
        var services = new ServiceCollection();
        services.AddScoped<IZeroCommandHandler<DuplicateCommand>, DuplicateCommandHandler<int>>();

        ServiceCollectionExtensions.RegisterHandlerTypes(
            services, [typeof(DuplicateCommandHandler<string>)], IsCommandHandler);

        var descriptor = Assert.Single(services, d => d.ServiceType == typeof(IZeroCommandHandler<DuplicateCommand>));
        Assert.Equal(typeof(DuplicateCommandHandler<int>), descriptor.ImplementationType);
    }

    [Fact]
    public void TwoHandlersForTheSameMessageShouldThrow()
    {
        var services = new ServiceCollection();

        var ex = Assert.Throws<InvalidOperationException>(() =>
            ServiceCollectionExtensions.RegisterHandlerTypes(
                services,
                [typeof(DuplicateCommandHandler<int>), typeof(DuplicateCommandHandler<string>)],
                IsCommandHandler));

        Assert.Contains(nameof(DuplicateCommand), ex.Message);
    }

    [Fact]
    public async Task BusShouldResolveFromScopeWithScopeValidation()
    {
        var services = new ServiceCollection();
        services.AddZeroCqrs(typeof(HandlerRegistrationTests));

        await using var provider = services.BuildServiceProvider(validateScopes: true);
        await using var scope = provider.CreateAsyncScope();

        var bus = scope.ServiceProvider.GetRequiredService<IZeroQueryBus>();
        Assert.Equal(1, await bus.Ask(new QueryWithInternalHandler()));
    }
}

public sealed record DuplicateCommand : IZeroCommand;

// Open generic: skipped by the assembly scan. Its closed forms are two distinct
// handler types for the same message, used to test duplicate detection.
public sealed class DuplicateCommandHandler<T> : IZeroCommandHandler<DuplicateCommand>
{
    public Task Execute(DuplicateCommand command, CancellationToken ct = default) => Task.CompletedTask;
}
