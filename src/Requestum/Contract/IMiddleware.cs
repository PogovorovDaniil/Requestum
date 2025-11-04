using Requestum.Middleware;
using System.Runtime.CompilerServices;

namespace Requestum.Contract;

/// <summary>
/// Base interface for all middleware types used in Requestum.
/// </summary>
public interface IMiddleware<TRequest, TResponse>;

public interface IBaseQueryMiddleware;

/// <summary>
/// Marker interface for query middleware. Do not use unless you know what you're doing.
/// Use <see cref="IQueryMiddleware{TQuery, TResponse}"/> or <see cref="IAsyncQueryMiddleware{TQuery, TResponse}"/> instead.
/// </summary>
public interface IBaseQueryMiddleware<TRequest, TResponse> : IMiddleware<TRequest, TResponse>, IBaseQueryMiddleware;

public interface IBaseCommandMiddleware;

/// <summary>
/// Marker interface for command middleware. Do not use unless you know what you're doing.
/// Use <see cref="ICommandMiddleware{TCommand, TResponse}"/> or <see cref="IAsyncCommandMiddleware{TCommand, TResponse}"/> instead.
/// </summary>
public interface IBaseCommandMiddleware<TRequest, TResponse> : IMiddleware<TRequest, TResponse>, IBaseCommandMiddleware;

/// <summary>
/// Base interface for all middleware types used in Requestum.
/// </summary>
public interface IBaseMiddleware<TRequest, TResponse>
{
    /// <summary>
    /// Processes the request and passes control to the next middleware or handler.
    /// </summary>
    /// <param name="request">The request being processed.</param>
    /// <param name="next">A delegate that invokes the next handler in the pipeline.</param>
    /// <returns>The result of processing the request.</returns>
    TResponse Invoke(TRequest request, RequestNextDelegate<TRequest, TResponse> next);
}

/// <summary>
/// Base interface for all middleware types used in Requestum.
/// </summary>
public interface IAsyncBaseMiddleware<TRequest, TResponse>
{
    /// <summary>
    /// Asynchronously processes the request and passes control to the next middleware or handler.
    /// </summary>
    /// <param name="request">The request being processed.</param>
    /// <param name="next">A delegate that invokes the next handler in the pipeline.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The result of processing the request.</returns>
    Task<TResponse> InvokeAsync(TRequest request, AsyncRequestNextDelegate<TRequest, TResponse> next, CancellationToken cancellationToken = default);
}

/// <summary>
/// Defines middleware for synchronous request processing.
/// </summary>
public interface IRequestMiddleware<TRequest, TResponse> : IBaseMiddleware<TRequest, TResponse>, IBaseCommandMiddleware<TRequest, TResponse>, IBaseQueryMiddleware<TRequest, TResponse>;

/// <summary>
/// Defines middleware for asynchronous request processing.
/// </summary>
public interface IAsyncRequestMiddleware<TRequest, TResponse> : IAsyncBaseMiddleware<TRequest, TResponse>, IBaseCommandMiddleware<TRequest, TResponse>, IBaseQueryMiddleware<TRequest, TResponse>;

/// <summary>
/// Defines middleware for synchronous query processing.
/// </summary>
public interface IQueryMiddleware<TQuery, TResponse> : IBaseMiddleware<TQuery, TResponse>, IBaseQueryMiddleware<TQuery, TResponse>;

/// <summary>
/// Defines middleware for asynchronous query processing.
/// </summary>
public interface IAsyncQueryMiddleware<TQuery, TResponse> : IAsyncBaseMiddleware<TQuery, TResponse>, IBaseQueryMiddleware<TQuery, TResponse>;

/// <summary>
/// Defines middleware for synchronous command processing.
/// </summary>
public interface ICommandMiddleware<TCommand, TResponse> : IBaseMiddleware<TCommand, TResponse>, IBaseCommandMiddleware<TCommand, TResponse>;

/// <summary>
/// Defines middleware for asynchronous command processing.
/// </summary>
public interface IAsyncCommandMiddleware<TCommand, TResponse> : IAsyncBaseMiddleware<TCommand, TResponse>, IBaseCommandMiddleware<TCommand, TResponse>;

/// <summary>
/// Represents a delegate that invokes the next middleware or final handler
/// in the synchronous request processing pipeline.
/// </summary>
/// <param name="request">The request being processed.</param>
/// <returns>The response returned by the next middleware or handler.</returns>
public readonly struct RequestNextDelegate<TRequest, TResponse>
{
    private readonly IMiddlewareDelegate<TRequest, TResponse> middleware;
    private readonly CancellationToken cancellationToken;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal RequestNextDelegate(IMiddlewareDelegate<TRequest, TResponse> middleware, CancellationToken cancellationToken)
    {
        this.middleware = middleware;
        this.cancellationToken = cancellationToken;
    }

    public TResponse Invoke(TRequest request) => middleware.Invoke(request, cancellationToken).GetAwaiter().GetResult();
}

/// <summary>
/// Represents a delegate that invokes the next middleware or final handler
/// in the asynchronous request processing pipeline.
/// </summary>
/// <param name="request">The request being processed.</param>
/// <param name="cancellationToken">The cancellation token used to cancel the operation.</param>
/// <returns>The response returned by the next middleware or handler.</returns>
public readonly struct AsyncRequestNextDelegate<TRequest, TResponse>
{
    private readonly IMiddlewareDelegate<TRequest, TResponse> middleware;
    private readonly CancellationToken cancellationToken;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal AsyncRequestNextDelegate(IMiddlewareDelegate<TRequest, TResponse> middleware, CancellationToken cancellationToken)
    {
        this.middleware = middleware;
        this.cancellationToken = cancellationToken;
    }

    public Task<TResponse> InvokeAsync(TRequest request) => middleware.Invoke(request, cancellationToken);
}