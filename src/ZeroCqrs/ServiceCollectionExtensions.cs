using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ZeroCqrs;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddZeroCqrsQueries(
        this IServiceCollection services,
        params Type[] handlerAssemblyMarkerTypes)
    {
        services.TryAddScoped<CqrsZeroQueryBus>();
        services.TryAddScoped<IZeroQueryBus>(sp => sp.GetRequiredService<CqrsZeroQueryBus>());

        RegisterHandlers(
            services,
            handlerAssemblyMarkerTypes,
            i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IZeroQueryHandler<,>));
        return services;
    }

    public static IServiceCollection AddZeroCqrsCommands(
        this IServiceCollection services,
        params Type[] handlerAssemblyMarkerTypes)
    {
        services.TryAddScoped<CqrsZeroCommandBus>();
        services.TryAddScoped<IZeroCommandBus>(sp => sp.GetRequiredService<CqrsZeroCommandBus>());

        RegisterHandlers(
            services,
            handlerAssemblyMarkerTypes,
            i => i.IsGenericType && (
                i.GetGenericTypeDefinition() == typeof(IZeroCommandHandler<>) ||
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
            : [Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly()];

        RegisterHandlerTypes(services, assemblies.SelectMany(GetLoadableTypes), isHandlerInterfacePredicate);
    }

    // Registers each closed handler interface found on the candidate types.
    // - open generic types are skipped (they cannot be registered as closed services);
    // - two scanned types handling the same message is an error (instead of "last one wins");
    // - an interface already registered (previous call, or the user's own registration) is kept.
    internal static void RegisterHandlerTypes(
        IServiceCollection services,
        IEnumerable<Type> candidateTypes,
        Func<Type, bool> isHandlerInterfacePredicate)
    {
        var registrations = candidateTypes
            .Where(t => t is { IsAbstract: false, IsInterface: false, IsGenericTypeDefinition: false })
            .SelectMany(t => t.GetInterfaces()
                .Where(isHandlerInterfacePredicate)
                .Select(i => (Service: i, Implementation: t)))
            .GroupBy(r => r.Service);

        foreach (var group in registrations)
        {
            var implementations = group.Select(r => r.Implementation).Distinct().ToArray();
            if (implementations.Length > 1)
            {
                throw new InvalidOperationException(
                    $"Several handlers found for '{group.Key}': " +
                    $"{string.Join(", ", implementations.Select(t => t.FullName))}. " +
                    "A message must have exactly one handler.");
            }

            services.TryAddScoped(group.Key, implementations[0]);
        }
    }

    private static IEnumerable<Type> GetLoadableTypes(Assembly assembly)
    {
        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException ex)
        {
            // Some types could not be loaded (missing optional dependency): keep the others.
            return ex.Types.Where(t => t is not null)!;
        }
    }
}
