using System.Runtime.CompilerServices;

namespace Requestum.Middleware.Application;

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
