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

internal readonly struct MiddlewareRetryAsyncDelegate<TRequest, TResponse>(
    int retryCount,
    IMiddlewareDelegate<TRequest, TResponse> next) : IMiddlewareDelegate<TRequest, TResponse>
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public async Task<TResponse> Invoke(TRequest request, CancellationToken cancellationToken = default)
    {
        List<Exception> exceptions = [];
        for (int i = 0; i < retryCount; i++)
        {
            try
            {
                return await next.Invoke(request, cancellationToken);
            }
            catch (Exception ex)
            {
                exceptions.Add(ex);
            }
        }
        throw new AggregateException(exceptions.ToArray());
    }
}

internal readonly struct MiddlewareTimeoutAsyncDelegate<TRequest, TResponse>(
    TimeSpan timeout,
    IMiddlewareDelegate<TRequest, TResponse> next) : IMiddlewareDelegate<TRequest, TResponse>
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public async Task<TResponse> Invoke(TRequest request, CancellationToken cancellationToken = default)
    {
        using var timeoutCts = new CancellationTokenSource(timeout);
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(
            cancellationToken, 
            timeoutCts.Token);

        try
        {
            return await next.Invoke(request, linkedCts.Token);
        }
        catch (OperationCanceledException) when (timeoutCts.IsCancellationRequested)
        {
            throw new TimeoutException();
        }
    }
}