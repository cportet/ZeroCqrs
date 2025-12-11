using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ZeroCqrs.Sample.Commands;
using ZeroCqrs.Sample.Repository;

namespace ZeroCqrs.Sample;

public class Startup
{
    public static void ConfigureServices(HostBuilderContext context, IServiceCollection services)
    {
        var userRepositoryConfiguration = new UserRepository();

        services.AddSingleton<UserRepository>(userRepositoryConfiguration);
        services.AddZeroCqrs();

        services.AddHostedService<Runner>();
    }
}
