using Requestum.Contract;
using System.Runtime.CompilerServices;

namespace Requestum.Middleware;

internal readonly struct QueryMiddlewareDelegate<TQuery, TResponse>(IQueryHandler<TQuery, TResponse> commandHandler) : IMiddlewareDelegate<TQuery, TResponse>
    where TQuery : IQuery<TResponse>
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task<TResponse> Invoke(TQuery request, CancellationToken cancellationToken = default)
    {
        return new ValueTask<TResponse>(commandHandler.Handle(request)).AsTask();
    }
}

internal readonly struct QueryMiddlewareAsyncDelegate<TQuery, TResponse>(IAsyncQueryHandler<TQuery, TResponse> commandHandler) : IMiddlewareDelegate<TQuery, TResponse>
    where TQuery : IQuery<TResponse>
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task<TResponse> Invoke(TQuery request, CancellationToken cancellationToken = default)
    {
        return commandHandler.HandleAsync(request, cancellationToken);
    }
}