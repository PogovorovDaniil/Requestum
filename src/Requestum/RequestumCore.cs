using Microsoft.Extensions.DependencyInjection;
using Requestum.Contract;
using Requestum.Middleware;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace Requestum;

public sealed partial class RequestumCore(IServiceProvider serviceProvider) : IRequestum
{
    private static ConcurrentDictionary<Type, object> cachedRequests = [];
    private static Dictionary<Type, int> retryPolicy = [];
    private static Dictionary<Type, TimeSpan> timeoutPolicy = [];

    public string[] GlobalTags { get; init; } = [];
    public bool RequireEventHandlers { get; internal set; }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private IMiddlewareDelegate<TRequest, TResponse> BuildCommandMiddleware<TRequest, TResponse>(IMiddlewareDelegate<TRequest, TResponse> middlewareDelegate, Type handlerType, TRequest request)
    {
        var baseMiddlewares = serviceProvider.GetServices<IBaseCommandMiddleware<TRequest, TResponse>>();

        if (timeoutPolicy.TryGetValue(handlerType, out var timeout))
            middlewareDelegate = new MiddlewareTimeoutAsyncDelegate<TRequest, TResponse>(timeout, middlewareDelegate);

        if (retryPolicy.TryGetValue(handlerType, out var retryCount))
            middlewareDelegate = new MiddlewareRetryAsyncDelegate<TRequest, TResponse>(retryCount, middlewareDelegate);

        string[] tags = request is ITaggedRequest taggedRequest ? taggedRequest.Tags : [];
        if (tags.Length + GlobalTags.Length > 0)
        {
            foreach(var tag in GlobalTags.Concat(tags))
            {
                baseMiddlewares = baseMiddlewares.Concat(serviceProvider.GetKeyedService<IEnumerable<IBaseCommandMiddleware<TRequest, TResponse>>>(tag) ?? []);
            }
        }

        foreach (var baseMiddleware in baseMiddlewares)
        {
            middlewareDelegate = baseMiddleware switch
            {
                IBaseMiddleware<TRequest, TResponse> middleware => new MiddlewareDelegate<TRequest, TResponse>(middleware, middlewareDelegate),
                IAsyncBaseMiddleware<TRequest, TResponse> asyncMiddleware => new MiddlewareAsyncDelegate<TRequest, TResponse>(asyncMiddleware, middlewareDelegate),
                _ => middlewareDelegate
            };
        }

        return middlewareDelegate;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private IMiddlewareDelegate<TRequest, TResponse> BuildQueryMiddleware<TRequest, TResponse>(IMiddlewareDelegate<TRequest, TResponse> middlewareDelegate, Type handlerType, TRequest request)
    {
        var baseMiddlewares = serviceProvider.GetServices<IBaseQueryMiddleware<TRequest, TResponse>>();

        if (timeoutPolicy.TryGetValue(handlerType, out var timeout))
            middlewareDelegate = new MiddlewareTimeoutAsyncDelegate<TRequest, TResponse>(timeout, middlewareDelegate);

        if (retryPolicy.TryGetValue(handlerType, out var retryCount))
            middlewareDelegate = new MiddlewareRetryAsyncDelegate<TRequest, TResponse>(retryCount, middlewareDelegate);

        string[] tags = request is ITaggedRequest taggedRequest ? taggedRequest.Tags : [];
        if (tags.Length + GlobalTags.Length > 0)
        {
            foreach (var tag in GlobalTags.Concat(tags))
            {
                baseMiddlewares = baseMiddlewares.Concat(serviceProvider.GetKeyedService<IEnumerable<IBaseQueryMiddleware<TRequest, TResponse>>>(tag) ?? []);
            }
        }
        foreach (var baseMiddleware in baseMiddlewares)
        {
            middlewareDelegate = baseMiddleware switch
            {
                IBaseMiddleware<TRequest, TResponse> middleware => new MiddlewareDelegate<TRequest, TResponse>(middleware, middlewareDelegate),
                IAsyncBaseMiddleware<TRequest, TResponse> asyncMiddleware => new MiddlewareAsyncDelegate<TRequest, TResponse>(asyncMiddleware, middlewareDelegate),
                _ => middlewareDelegate
            };
        }

        return middlewareDelegate;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private IBaseHandler<TRequest>? GetHandler<TRequest>(TRequest request)
        where TRequest : IBaseRequest
    {
        if (request is ITaggedRequest taggedRequest)
        {
            IEnumerable<string> tags = taggedRequest.Tags;
            if (GlobalTags.Length > 0) tags = GlobalTags.Concat(tags);

            foreach (var tag in tags)
            {
                var handler = serviceProvider.GetKeyedService<IBaseHandler<TRequest>>(tag);
                if (handler is not null) return handler;
            }
        }
        else if(GlobalTags.Length > 0)
        {
            foreach (var tag in GlobalTags)
            {
                var handler = serviceProvider.GetKeyedService<IBaseHandler<TRequest>>(tag);
                if (handler is not null) return handler;
            }
        }

        return serviceProvider.GetService<IBaseHandler<TRequest>>();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private IEnumerable<IBaseHandler<TRequest>>? GetHandlers<TRequest>(TRequest request)
        where TRequest : IBaseRequest
    {
        IEnumerable<IBaseHandler<TRequest>> handlers = serviceProvider.GetServices<IBaseHandler<TRequest>>();
        foreach (var tag in GlobalTags.Concat(request is ITaggedRequest taggedRequest ? taggedRequest.Tags : []))
        {
            handlers = handlers.Concat(serviceProvider.GetKeyedServices<IBaseHandler<TRequest>>(tag));
        }
        return handlers;
    }

    public void AddHandlerRetry(Type type, int retryCount) => retryPolicy[type] = retryCount;
    public void AddHandlerTimeout(Type type, TimeSpan timeout) => timeoutPolicy[type] = timeout;
}
