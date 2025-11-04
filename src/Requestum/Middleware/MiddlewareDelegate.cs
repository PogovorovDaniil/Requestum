using Requestum.Contract;
using System.Runtime.CompilerServices;

namespace Requestum.Middleware;

internal readonly struct MiddlewareDelegate<TRequest, TResponse>(
    IBaseMiddleware<TRequest, TResponse> requestMiddleware,
    IMiddlewareDelegate<TRequest, TResponse> next) : IMiddlewareDelegate<TRequest, TResponse>
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task<TResponse> Invoke(TRequest request, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(requestMiddleware.Invoke(request, new RequestNextDelegate<TRequest, TResponse>(next, cancellationToken)));
    }
}

internal readonly struct MiddlewareAsyncDelegate<TRequest, TResponse>(
    IAsyncBaseMiddleware<TRequest, TResponse> asyncRequestMiddleware,
    IMiddlewareDelegate<TRequest, TResponse> next) : IMiddlewareDelegate<TRequest, TResponse>
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task<TResponse> Invoke(TRequest request, CancellationToken cancellationToken = default)
    {
        return asyncRequestMiddleware.InvokeAsync(request, new AsyncRequestNextDelegate<TRequest, TResponse>(next, cancellationToken), cancellationToken);
    }
}