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
        var baseMiddlewares = serviceProvider.GetService<IEnumerable<IBaseCommandMiddleware<TRequest, TResponse>>>() ?? [];

        string? tag = request is ITaggedRequest taggedRequest ? taggedRequest.Tag : null;
        if (tag is not null) baseMiddlewares = baseMiddlewares.Concat(serviceProvider.GetKeyedService<IEnumerable<IBaseCommandMiddleware<TRequest, TResponse>>>(tag) ?? []);

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
        var baseMiddlewares = serviceProvider.GetService<IEnumerable<IBaseQueryMiddleware<TRequest, TResponse>>>() ?? [];

        string? tag = request is ITaggedRequest taggedRequest ? taggedRequest.Tag : null;
        if (tag is not null) baseMiddlewares = baseMiddlewares.Concat(serviceProvider.GetKeyedService<IEnumerable<IBaseQueryMiddleware<TRequest, TResponse>>>(tag) ?? []);

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
            return serviceProvider.GetKeyedServices<IBaseHandler<TRequest>>(taggedRequest.Tag).SingleOrDefault();

        return serviceProvider.GetService<IBaseHandler<TRequest>>();
    }

    private IEnumerable<IBaseHandler<TRequest>>? GetHandlers<TRequest>(TRequest request)
        where TRequest : IBaseRequest
    {
        if (request is ITaggedRequest taggedRequest)
            return serviceProvider.GetKeyedServices<IBaseHandler<TRequest>>(taggedRequest.Tag);

        return serviceProvider.GetServices<IBaseHandler<TRequest>>();
    }
}
