using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ZeroCqrs;

/// <summary>
/// Registers the ZeroCqrs buses and scans assemblies for handlers.
/// </summary>
/// <remarks>
/// Buses and handlers are registered as scoped. Each message type must have exactly one handler;
/// an interface already registered before the call is kept. Calling the methods several times is safe.
/// Without marker type, the entry assembly of the application is scanned.
/// </remarks>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers <see cref="IZeroQueryBus"/> and every <see cref="IZeroQueryHandler{TQuery, TResponse}"/>
    /// found in the assemblies of <paramref name="handlerAssemblyMarkerTypes"/>.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="handlerAssemblyMarkerTypes">Any type of each assembly to scan. Empty: the entry assembly.</param>
    /// <returns><paramref name="services"/>, for chaining.</returns>
    /// <exception cref="InvalidOperationException">Two handlers were found for the same query type.</exception>
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

    /// <summary>
    /// Registers <see cref="IZeroCommandBus"/> and every <see cref="IZeroCommandHandler{TCommand}"/> and
    /// <see cref="IZeroCommandHandler{TCommand, TResponse}"/> found in the assemblies of
    /// <paramref name="handlerAssemblyMarkerTypes"/>.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="handlerAssemblyMarkerTypes">Any type of each assembly to scan. Empty: the entry assembly.</param>
    /// <returns><paramref name="services"/>, for chaining.</returns>
    /// <exception cref="InvalidOperationException">Two handlers were found for the same command type.</exception>
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
                i.GetGenericTypeDefinition() == typeof(IZeroCommandHandler<,>)));
        return services;
    }

    /// <summary>
    /// Registers both buses and all query and command handlers: same as calling
    /// <see cref="AddZeroCqrsQueries"/> and <see cref="AddZeroCqrsCommands"/>.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="handlerAssemblyMarkerTypes">Any type of each assembly to scan. Empty: the entry assembly.</param>
    /// <returns><paramref name="services"/>, for chaining.</returns>
    /// <exception cref="InvalidOperationException">Two handlers were found for the same message type.</exception>
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
