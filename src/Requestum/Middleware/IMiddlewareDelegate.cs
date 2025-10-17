using System.Runtime.CompilerServices;

namespace Requestum.Middleware;

internal interface IMiddlewareDelegate<TRequest, TResponse>
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    Task<TResponse> Invoke(TRequest request, CancellationToken cancellationToken = default);
}
