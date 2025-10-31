using Microsoft.Extensions.DependencyInjection;
using Requestum.Contract;
using Requestum.Middleware;
using System.Collections.Concurrent;

namespace Requestum;

public sealed partial class RequestumCore(IServiceProvider serviceProvider) : IRequestum
{
    private static ConcurrentDictionary<Type, object> cachedRequests = [];
    public bool RequireEventHandlers { get; internal set; }

    /// <summary>
    /// Builds the middleware pipeline by chaining registered middleware components.
    /// </summary>
    /// <param name="middlewareDelegate">The initial middleware delegate.</param>
    /// <returns>The configured middleware pipeline delegate.</returns>
    private IMiddlewareDelegate<TRequest, TResponse> BuildMiddleware<TRequest, TResponse>(IMiddlewareDelegate<TRequest, TResponse> middlewareDelegate, RequestType requestType)
    {
        if (serviceProvider is null) return middlewareDelegate;

        var baseMiddlewares = serviceProvider.GetKeyedService<IEnumerable<IBaseMiddleware<TRequest, TResponse>>>(requestType);
        if (baseMiddlewares is null) return middlewareDelegate;

        foreach (var baseMiddleware in baseMiddlewares)
        {
            middlewareDelegate = baseMiddleware switch
            {
                IRequestMiddleware<TRequest, TResponse> middleware => new MiddlewareDelegate<TRequest, TResponse>(middleware, middlewareDelegate),
                IAsyncRequestMiddleware<TRequest, TResponse> asyncMiddleware => new MiddlewareAsyncDelegate<TRequest, TResponse>(asyncMiddleware, middlewareDelegate),
                _ => middlewareDelegate
            };
        }

        return middlewareDelegate;
    }
}
