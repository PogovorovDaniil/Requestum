using Microsoft.Extensions.DependencyInjection.Extensions;
using Requestum;
using Requestum.Contract;
using Requestum.Policy;
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

            services.TryAddEnumerable(new ServiceDescriptor(typeof(IBaseHandler), handlerType, serviceLifetime));
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

        cfg.CustomHandlers.AddRange(GetHandlerTypes(cfg.HandlerAssemblies).Select(t => (t, cfg.Lifetime)));
        foreach (var service in cfg.CustomHandlers)
        {
            if (service.Lifetime == ServiceLifetime.Scoped && cfg.Lifetime != ServiceLifetime.Scoped)
                throw new RequestumException($"Cannot register a scoped handler '{service.HandlerType.Name}' when the global lifetime is not scoped.");

            RegisterHandler(services, service.Lifetime, service.HandlerType);
        }

        cfg.CustomMiddlewares.AddRange(GetMiddlewareTypes(cfg.MiddlewareAssemblies).Select(t => (t, cfg.Lifetime)));
        foreach (var service in cfg.CustomMiddlewares)
        {
            if (service.Lifetime == ServiceLifetime.Scoped && cfg.Lifetime != ServiceLifetime.Scoped)
                throw new RequestumException($"Cannot register a scoped middleware '{service.MiddlewareType.Name}' when the global lifetime is not scoped.");

            RegisterMiddleware(services, service.Lifetime, service.MiddlewareType);
        }

        services.TryAdd(new ServiceDescriptor(typeof(IRequestum), serviceProvider =>
        {
            return new RequestumCore(serviceProvider)
            {
                RequireEventHandlers = cfg.RequireEventHandlers,
                GlobalTags = cfg.GlobalTags,
            };
        }, cfg.Lifetime));

        return services;
    }

    public static IServiceProvider AutoRegisterRequestumPolicies(this IServiceProvider serviceProvider)
    {
        IRequestum requestum = serviceProvider.GetService<IRequestum>()!;
        var handlers = serviceProvider.GetServices<IBaseHandler>() ?? [];

        foreach (var handler in handlers)
        {
            var type = handler.GetType();

            RetryAttribute? retry;
            if ((retry = type.GetCustomAttribute<RetryAttribute>()) is not null) requestum.AddHandlerRetry(type, retry.RetryCount);

            TimeoutAttribute? timeout;
            if ((timeout = type.GetCustomAttribute<TimeoutAttribute>()) is not null) requestum.AddHandlerTimeout(type, TimeSpan.FromMilliseconds(timeout.Timeout));
        }

        return serviceProvider;
    }
}
