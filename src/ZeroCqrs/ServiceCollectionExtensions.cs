using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Reflection;

namespace ZeroCqrs;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddZeroCqrsQueries(
        this IServiceCollection services,
        params Type[] handlerAssemblyMarkerTypes)
    {
        services.AddScoped<CqrsQueryBus>();
        RegisterHandlers(
            services,
            handlerAssemblyMarkerTypes,
            i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IQueryHandler<,>));
        return services;
    }

    public static IServiceCollection AddZeroCqrsCommands(
        this IServiceCollection services,
        params Type[] handlerAssemblyMarkerTypes)
    {
        services.AddScoped<CqrsCommandBus>();
        RegisterHandlers(
            services,
            handlerAssemblyMarkerTypes,
            i => i.IsGenericType && (
                i.GetGenericTypeDefinition() == typeof(ICommandHandler<>) ||
                i.GetGenericTypeDefinition() == typeof(ICommandHandler<,>)));
        return services;
    }

    public static IServiceCollection AddZeroCqrs(
        this IServiceCollection services,
        params Type[] handlerAssemblyMarkerTypes)
        => services
            .AddZeroCqrsQueries(handlerAssemblyMarkerTypes)
            .AddZeroCqrsCommands(handlerAssemblyMarkerTypes);

    private static void RegisterHandlers(
        IServiceCollection services,
        Type[] markerTypes,
        Func<Type, bool> isHandlerInterfacePredicate)
    {
        var assemblies = markerTypes?.Length > 0
            ? markerTypes.Select(t => t.Assembly).Distinct()
            : [Assembly.GetCallingAssembly()];

        var handlerTypes = assemblies
            .SelectMany(a => a.GetTypes())
            .Where(t => !t.IsAbstract && !t.IsInterface &&
                        t.GetInterfaces().Any(isHandlerInterfacePredicate))
            .ToArray();

        foreach (var handlerType in handlerTypes)
        {
            var interfaces = handlerType
                .GetInterfaces()
                .Where(isHandlerInterfacePredicate)
                .ToArray();

            foreach (var i in interfaces)
            {
                services.AddScoped(i, handlerType);
            }
        }
    }
}