using Microsoft.Extensions.Hosting;
using ZeroCqrs.Sample;

await Host.CreateDefaultBuilder(args)
    .ConfigureServices(Startup.ConfigureServices)
    .Build()
    .RunAsync();