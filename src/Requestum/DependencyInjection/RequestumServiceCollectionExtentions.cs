using Microsoft.Extensions.DependencyInjection.Extensions;
using Requestum;
using Requestum.Contract;
using System.Reflection;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Delegate for configuring Requestum services.
/// </summary>
/// <param name="cfg">The <see cref="RequestumServiceConfiguration"/> instance to configure.</param>
public delegate void RequestumServiceConfigConfigurationBuilder(RequestumServiceConfiguration cfg);

/// <summary>
/// Extension methods for <see cref="IServiceCollection"/> to register Requestum services.
/// </summary>
public static class RequestumServiceCollectionExtentions
{
    private static readonly Type[] handlerInterfaceTypes =
        [
            typeof(ICommandHandler<>),
            typeof(IAsyncCommandHandler<>),
            typeof(ICommandHandler<,>),
            typeof(IAsyncCommandHandler<,>),
            typeof(IQueryHandler<,>),
            typeof(IAsyncQueryHandler<,>),
            typeof(IEventMessageReceiver<>),
            typeof(IAsyncEventMessageReceiver<>),
        ];

    private static Type[] GetHandlerTypes(IEnumerable<Assembly> assemblies) => assemblies
        .SelectMany(a => a.GetTypes())
        .Where(t => t.IsClass && !t.IsAbstract)
        .Where(t => typeof(IBaseHandler).IsAssignableFrom(t))
        .ToArray();

    private static Type[] GetMiddlewareTypes(IEnumerable<Assembly> assemblies) => assemblies
        .SelectMany(a => a.GetTypes())
        .Where(t => t.IsClass && !t.IsAbstract)
        .Where(t => t.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IMiddleware<,>)))
        .ToArray();

    private static Type[] GetHandlerRequestTypes(Type handlerType) => handlerType
        .GetInterfaces()
        .Where(i => i.IsInterface && i.IsGenericType)
        .Where(i => handlerInterfaceTypes.Contains(i.GetGenericTypeDefinition()))
        .Select(i => i.GenericTypeArguments[0])
        .ToArray();

    private static IEnumerable<string> GetHandlerTags(Type handlerType) => handlerType
        .GetCustomAttributes<HandlerTagAttribute>().Select(a => a.Tag);

    private static IEnumerable<string> GetMiddlewareTags(Type handlerType) => handlerType
        .GetCustomAttributes<MiddlewareTagAttribute>().Select(a => a.Tag);

    /// <summary>
    /// Registers all handler types found in the specified assemblies with the dependency injection container.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <param name="serviceLifetime">The service lifetime for the registered handlers.</param>
    /// <param name="assemblies">The assemblies to scan for handler types.</param>
    /// <remarks>
    /// Event message receivers are registered as enumerable services to support multiple handlers per event.
    /// Command and query handlers are registered as single services (one handler per request type).
    /// </remarks>
    public static void RegisterHandlers(IServiceCollection services, ServiceLifetime serviceLifetime, IEnumerable<Assembly> assemblies)
    {
        foreach (var handlerType in GetHandlerTypes(assemblies))
        {
            RegisterHandler(services, serviceLifetime, handlerType);
        }
    }

    private static void RegisterHandler(IServiceCollection services, ServiceLifetime serviceLifetime, Type handlerType)
    {
        foreach (var requestType in GetHandlerRequestTypes(handlerType))
        {
            var tags = GetHandlerTags(handlerType);

            if (tags.Any())
            {
                foreach(var tag in tags)
                {
                    services.TryAddEnumerable(new ServiceDescriptor(typeof(IBaseHandler<>).MakeGenericType(requestType), tag, handlerType, serviceLifetime));
                }
            }
            else
            {
                services.TryAddEnumerable(new ServiceDescriptor(typeof(IBaseHandler<>).MakeGenericType(requestType), handlerType, serviceLifetime));
            }
        }
    }

    /// <summary>
    /// Registers all middleware types found in the specified assemblies with the dependency injection container.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <param name="serviceLifetime">The service lifetime for the registered middlewares.</param>
    /// <param name="assemblies">The assemblies to scan for middleware types.</param>
    /// <remarks>
    /// Middlewares are registered as enumerable services to support multiple middlewares in the pipeline.
    /// </remarks>
    public static void RegisterMiddlewares(IServiceCollection services, ServiceLifetime serviceLifetime, IEnumerable<Assembly> assemblies)
    {
        foreach (var middlewareType in GetMiddlewareTypes(assemblies))
        {
            RegisterMiddleware(services, serviceLifetime, middlewareType);
        }
    }

    private static void RegisterMiddleware(IServiceCollection services, ServiceLifetime serviceLifetime, Type middlewareType)
    {
        var tags = GetMiddlewareTags(middlewareType);
        if (tags.Any())
        {
            foreach(var tag in tags)
            {
                if (typeof(IBaseCommandMiddleware).IsAssignableFrom(middlewareType))
                    services.TryAddEnumerable(new ServiceDescriptor(typeof(IBaseCommandMiddleware<,>), tag, middlewareType, serviceLifetime));
                if (typeof(IBaseQueryMiddleware).IsAssignableFrom(middlewareType))
                    services.TryAddEnumerable(new ServiceDescriptor(typeof(IBaseQueryMiddleware<,>), tag, middlewareType, serviceLifetime));
            }
        }
        else
        {
            if (typeof(IBaseCommandMiddleware).IsAssignableFrom(middlewareType))
                services.TryAddEnumerable(new ServiceDescriptor(typeof(IBaseCommandMiddleware<,>), middlewareType, serviceLifetime));
            if (typeof(IBaseQueryMiddleware).IsAssignableFrom(middlewareType))
                services.TryAddEnumerable(new ServiceDescriptor(typeof(IBaseQueryMiddleware<,>), middlewareType, serviceLifetime));
        }
    }

    /// <summary>
    /// Adds Requestum services to the specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <param name="configurationBuilder">An optional delegate to configure Requestum services.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    /// <exception cref="RequestumException">
    /// Thrown when attempting to register a scoped handler or middleware when the global lifetime is not scoped.
    /// </exception>
    /// <example>
    /// <code>
    /// services.AddRequestum(cfg =>
    /// {
    ///     cfg.Lifetime = ServiceLifetime.Scoped;
    ///     cfg.RequireEventHandlers = false;
    ///     cfg.Default(typeof(Program).Assembly);
    /// });
    /// </code>
    /// </example>
    public static IServiceCollection AddRequestum(this IServiceCollection services, RequestumServiceConfigConfigurationBuilder? configurationBuilder)
    {
        var cfg = new RequestumServiceConfiguration();
        configurationBuilder?.Invoke(cfg);

        foreach (var service in cfg.CustomHandlers)
        {
            if (service.Lifetime == ServiceLifetime.Scoped && cfg.Lifetime != ServiceLifetime.Scoped)
                throw new RequestumException($"Cannot register a scoped handler '{service.HandlerType.Name}' when the global lifetime is not scoped.");

            RegisterHandler(services, service.Lifetime, service.HandlerType);
        }

        foreach (var service in cfg.CustomMiddlewares)
        {
            if (service.Lifetime == ServiceLifetime.Scoped && cfg.Lifetime != ServiceLifetime.Scoped)
                throw new RequestumException($"Cannot register a scoped middleware '{service.MiddlewareType.Name}' when the global lifetime is not scoped.");

            RegisterMiddleware(services, service.Lifetime, service.MiddlewareType);
        }

        RegisterHandlers(services, cfg.Lifetime, cfg.HandlerAssemblies);
        RegisterMiddlewares(services, cfg.Lifetime, cfg.MiddlewareAssemblies);

        services.TryAdd(new ServiceDescriptor(typeof(IRequestum), serviceProvider =>
        {
            return new RequestumCore(serviceProvider)
            {
                RequireEventHandlers = cfg.RequireEventHandlers
            };
        }, cfg.Lifetime));

        return services;
    }
}
