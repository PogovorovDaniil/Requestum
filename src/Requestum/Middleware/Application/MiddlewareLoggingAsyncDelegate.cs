using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Requestum.Middleware.Application;

internal readonly struct MiddlewareLoggingAsyncDelegate<TRequest, TResponse>(
    ILogger logger,
    IMiddlewareDelegate<TRequest, TResponse> next) : IMiddlewareDelegate<TRequest, TResponse>
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public async Task<TResponse> Invoke(TRequest request, CancellationToken cancellationToken = default)
    {
        var sw = Stopwatch.StartNew();

        try
        {
            logger.LogInformation("Handling {RequestType}", typeof(TRequest).Name);

            var response = await next.Invoke(request, cancellationToken);

            sw.Stop();
            logger.LogInformation("Handled {RequestType} in {Elapsed} ms",
                typeof(TRequest).Name, sw.ElapsedMilliseconds);

            return response;
        }
        catch (Exception ex)
        {
            sw.Stop();
            logger.LogError(ex,
                "Error handling {RequestType} after {Elapsed} ms",
                typeof(TRequest).Name, sw.ElapsedMilliseconds);

            throw;
        }
    }
}