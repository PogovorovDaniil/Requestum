using Requestum.Contract;
using System.Reflection;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Configuration options for registering Requestum services with the dependency injection container.
/// </summary>
public sealed class RequestumServiceConfiguration
{
    /// <summary>
    /// Gets or sets the default service lifetime for handlers and middlewares.
    /// Default value is <see cref="ServiceLifetime.Transient"/>.
    /// </summary>
    /// <value>
    /// The service lifetime to use for all registered handlers and middlewares unless otherwise specified.
    /// </value>
    public ServiceLifetime Lifetime { get; set; } = ServiceLifetime.Transient;

    /// <summary>
    /// Gets or sets a value indicating whether event handlers are required to be registered for event messages.
    /// When <c>true</c>, attempting to publish an event message without any registered receivers will throw an exception.
    /// When <c>false</c>, publishing an event message without receivers will be silently ignored.
    /// Default value is <c>true</c>.
    /// </summary>
    /// <value>
    /// <c>true</c> if at least one event handler must be registered; otherwise, <c>false</c>.
    /// </value>
    public bool RequireEventHandlers { get; set; } = true;

    /// <summary>
    /// Gets or sets the global tags that will be applied to all requests processed by Requestum.
    /// These tags can be used for cross-cutting concerns such as logging, monitoring, or filtering.
    /// Default value is an empty array.
    /// </summary>
    /// <value>
    /// An array of string tags to be applied globally to all requests.
    /// </value>
    public string[] GlobalTags { get; set; } = [];

    internal List<Assembly> HandlerAssemblies { get; set; } = [];
    internal List<Assembly> MiddlewareAssemblies { get; set; } = [];
    internal List<Assembly> EventMessageAssemblies { get; set; } = [];
    internal List<(Type HandlerType, ServiceLifetime Lifetime)> CustomHandlers { get; set; } = [];
    internal List<(Type MiddlewareType, ServiceLifetime Lifetime)> CustomMiddlewares { get; set; } = [];

    /// <summary>
    /// Registers both handlers and middlewares from the specified assemblies.
    /// This is a convenience method that calls both <see cref="RegisterHandlers"/> and <see cref="RegisterMiddlewares"/>.
    /// </summary>
    /// <param name="assemblies">The assemblies to scan for handlers and middlewares.</param>
    /// <returns>The current <see cref="RequestumServiceConfiguration"/> instance for method chaining.</returns>
    public RequestumServiceConfiguration Default(params Assembly[] assemblies)
    {
        RegisterHandlers(assemblies);
        RegisterMiddlewares(assemblies);
        return this;
    }

    /// <summary>
    /// Registers all handlers found in the specified assemblies.
    /// Scans for types implementing <see cref="ICommandHandler{TCommand}"/>, <see cref="IAsyncCommandHandler{TCommand}"/>,
    /// <see cref="IQueryHandler{TQuery, TResponse}"/>, <see cref="IAsyncQueryHandler{TQuery, TResponse}"/>,
    /// <see cref="IEventMessageReceiver{TMessage}"/>, and <see cref="IAsyncEventMessageReceiver{TMessage}"/>.
    /// </summary>
    /// <param name="assemblies">The assemblies to scan for handlers.</param>
    /// <returns>The current <see cref="RequestumServiceConfiguration"/> instance for method chaining.</returns>
    public RequestumServiceConfiguration RegisterHandlers(params Assembly[] assemblies)
    {
        HandlerAssemblies.AddRange(assemblies);
        return this;
    }

    /// <summary>
    /// Registers a specific handler type with the specified service lifetime.
    /// </summary>
    /// <typeparam name="THandler">The handler type to register. Must implement <see cref="IBaseHandler"/>.</typeparam>
    /// <param name="serviceLifetime">The service lifetime for this handler. Default is <see cref="ServiceLifetime.Transient"/>.</param>
    /// <returns>The current <see cref="RequestumServiceConfiguration"/> instance for method chaining.</returns>
    public RequestumServiceConfiguration RegisterHandler<THandler>(ServiceLifetime? serviceLifetime = null) where THandler : IBaseHandler =>
        RegisterHandler(typeof(THandler), serviceLifetime ?? Lifetime);

    /// <summary>
    /// Registers a specific handler type with the specified service lifetime.
    /// </summary>
    /// <param name="handlerType">The handler type to register.</param>
    /// <param name="serviceLifetime">The service lifetime for this handler. Default is <see cref="ServiceLifetime.Transient"/>.</param>
    /// <returns>The current <see cref="RequestumServiceConfiguration"/> instance for method chaining.</returns>
    public RequestumServiceConfiguration RegisterHandler(Type handlerType, ServiceLifetime? serviceLifetime = null)
    {
        CustomHandlers.Add((handlerType, serviceLifetime ?? Lifetime));
        return this;
    }

    /// <summary>
    /// Registers all middlewares found in the specified assemblies.
    /// Scans for types implementing <see cref="IRequestMiddleware{TRequest, TResponse}"/> 
    /// and <see cref="IAsyncRequestMiddleware{TRequest, TResponse}"/>.
    /// </summary>
    /// <param name="assemblies">The assemblies to scan for middlewares.</param>
    /// <returns>The current <see cref="RequestumServiceConfiguration"/> instance for method chaining.</returns>
    public RequestumServiceConfiguration RegisterMiddlewares(params Assembly[] assemblies)
    {
        MiddlewareAssemblies.AddRange(assemblies);
        return this;
    }

    /// <summary>
    /// Registers a specific middleware type with the specified service lifetime.
    /// </summary>
    /// <param name="middlewareType">The middleware type to register.</param>
    /// <param name="serviceLifetime">The service lifetime for this middleware. Default is <see cref="ServiceLifetime.Transient"/>.</param>
    /// <returns>The current <see cref="RequestumServiceConfiguration"/> instance for method chaining.</returns>
    public RequestumServiceConfiguration RegisterMiddleware(Type middlewareType, ServiceLifetime? serviceLifetime = null)
    {
        CustomMiddlewares.Add((middlewareType, serviceLifetime ?? Lifetime));
        return this;
    }
}
