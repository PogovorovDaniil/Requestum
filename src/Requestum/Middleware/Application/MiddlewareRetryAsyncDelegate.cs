using System.Runtime.CompilerServices;

namespace Requestum.Middleware.Application;

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
