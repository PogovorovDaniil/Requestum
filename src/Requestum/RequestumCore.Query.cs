using Requestum.Contract;
using Requestum.Middleware;

namespace Requestum;

public partial class RequestumCore
{
    public TResponse Handle<TQuery, TResponse>(TQuery query)
        where TQuery : IQuery<TResponse>
    {
        var handler = GetHandler(query);
        return handler switch
        {
            IQueryHandler<TQuery, TResponse> queryHandler =>
                BuildQueryMiddleware(new QueryMiddlewareDelegate<TQuery, TResponse>(queryHandler), query)
                    .Invoke(query)
                    .GetAwaiter()
                    .GetResult()!,
            IAsyncQueryHandler<TQuery, TResponse> asyncQueryHandler =>
                BuildQueryMiddleware(new QueryMiddlewareAsyncDelegate<TQuery, TResponse>(asyncQueryHandler), query)
                    .Invoke(query)
                    .GetAwaiter()
                    .GetResult()!,
            _ => throw new RequestumException($"No handler registered for query type '{typeof(TQuery).Name}'."),
        };
    }

    public Task<TResponse> HandleAsync<TQuery, TResponse>(TQuery query, CancellationToken cancellationToken = default)
        where TQuery : IQuery<TResponse>
    {
        var handler = GetHandler(query);
        return handler switch
        {
            IAsyncQueryHandler<TQuery, TResponse> asyncQueryHandler =>
                BuildQueryMiddleware(new QueryMiddlewareAsyncDelegate<TQuery, TResponse>(asyncQueryHandler), query)
                    .Invoke(query),
            IQueryHandler<TQuery, TResponse> queryHandler =>
                BuildQueryMiddleware(new QueryMiddlewareDelegate<TQuery, TResponse>(queryHandler), query)
                    .Invoke(query),
            _ => throw new RequestumException($"No handler registered for query type '{typeof(TQuery).Name}'."),
        };
    }

    public TResponse Handle<TQuery, TResponse>() where TQuery : IQuery<TResponse>, new() =>
        Handle<TQuery, TResponse>((TQuery)cachedRequests.GetOrAdd(typeof(TQuery), new TQuery()));

    public Task<TResponse> HandleAsync<TQuery, TResponse>(CancellationToken cancellationToken = default) where TQuery : IQuery<TResponse>, new() =>
        HandleAsync<TQuery, TResponse>((TQuery)cachedRequests.GetOrAdd(typeof(TQuery), new TQuery()));
}
