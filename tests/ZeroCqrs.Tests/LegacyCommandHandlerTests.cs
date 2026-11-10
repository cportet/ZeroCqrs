using Microsoft.Extensions.DependencyInjection;

namespace ZeroCqrs.Tests;

public class LegacyCommandHandlerTests
{
    [Fact]
    public async Task HandlerImplementingObsoleteInterfaceShouldStillBeDispatched()
    {
        var services = new ServiceCollection();
        services.AddZeroCqrsCommands(typeof(LegacyCommandHandlerTests));

        var bus = services.BuildServiceProvider().GetRequiredService<IZeroCommandBus>();

        Assert.Equal("legacy", await bus.Send(new LegacyCommand()));
    }
}

public sealed record LegacyCommand : IZeroCommand<string>;

#pragma warning disable CS0618 // ICommandHandler<,> is obsolete: this test checks it still works
public sealed class LegacyCommandHandler : ICommandHandler<LegacyCommand, string>
#pragma warning restore CS0618
{
    public Task<string> Execute(LegacyCommand command, CancellationToken ct = default) => Task.FromResult("legacy");
}
