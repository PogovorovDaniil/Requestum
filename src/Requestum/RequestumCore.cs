using Microsoft.Extensions.DependencyInjection;
using Requestum.Contract;
using Requestum.Middleware;
using System.Collections.Concurrent;

namespace Requestum;

public sealed partial class RequestumCore(IServiceProvider serviceProvider) : IRequestum
{
    private static ConcurrentDictionary<Type, object> cachedRequests = [];
    public bool RequireEventHandlers { get; internal set; }

    private IMiddlewareDelegate<TRequest, TResponse> BuildCommandMiddleware<TRequest, TResponse>(IMiddlewareDelegate<TRequest, TResponse> middlewareDelegate, TRequest request)
    {
        var baseMiddlewares = serviceProvider.GetServices<IBaseCommandMiddleware<TRequest, TResponse>>();

        string[] tags = request is ITaggedRequest taggedRequest ? taggedRequest.Tags : [];
        if (tags.Length > 0)
        {
            foreach(var tag in tags)
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

    private IMiddlewareDelegate<TRequest, TResponse> BuildQueryMiddleware<TRequest, TResponse>(IMiddlewareDelegate<TRequest, TResponse> middlewareDelegate, TRequest request)
    {
        var baseMiddlewares = serviceProvider.GetServices<IBaseQueryMiddleware<TRequest, TResponse>>();

        string[] tags = request is ITaggedRequest taggedRequest ? taggedRequest.Tags : [];
        if (tags.Length > 0)
        {
            foreach (var tag in tags)
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

    private IBaseHandler<TRequest>? GetHandler<TRequest>(TRequest request)
        where TRequest : IBaseRequest
    {
        if (request is ITaggedRequest taggedRequest)
        {
            foreach (var tag in taggedRequest.Tags)
            {
                var handler = serviceProvider.GetKeyedService<IBaseHandler<TRequest>>(tag);
                if (handler is not null) return handler;
            }
        }

        return serviceProvider.GetService<IBaseHandler<TRequest>>();
    }

    private IEnumerable<IBaseHandler<TRequest>>? GetHandlers<TRequest>(TRequest request)
        where TRequest : IBaseRequest
    {
        if (request is ITaggedRequest taggedRequest)
        {
            IEnumerable<IBaseHandler<TRequest>> handlers = [];
            foreach (var tag in taggedRequest.Tags)
            {
                handlers = handlers.Concat(serviceProvider.GetKeyedServices<IBaseHandler<TRequest>>(tag));
            }
            return handlers;
        }

        return serviceProvider.GetServices<IBaseHandler<TRequest>>();
    }
}
