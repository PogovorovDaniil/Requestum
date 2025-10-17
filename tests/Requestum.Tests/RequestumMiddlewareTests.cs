using Microsoft.Extensions.DependencyInjection;
using Requestum.Contract;

namespace Requestum.Tests;

/// <summary>
/// Test middleware implementations for validating middleware pipeline functionality.
/// </summary>
public class TestRequestMiddleware<TRequest, TResponse> : IRequestMiddleware<TRequest, TResponse>
{
    public static bool Called { get; private set; }

    public TResponse Invoke(TRequest request, RequestNextDelegate<TRequest, TResponse> next)
    {
        Called = true;
        return next.Invoke(request);
    }

    public static void Reset() => Called = false;
}

/// <summary>
/// Async test middleware implementation for validating asynchronous middleware pipeline functionality.
/// </summary>
public class TestAsyncRequestMiddleware<TRequest, TResponse> : IAsyncRequestMiddleware<TRequest, TResponse>
{
    public static bool Called { get; private set; }

    public async Task<TResponse> InvokeAsync(
        TRequest request,
        AsyncRequestNextDelegate<TRequest, TResponse> next,
        CancellationToken cancellationToken = default)
    {
        Called = true;
        return await next.InvokeAsync(request);
    }

    public static void Reset() => Called = false;
}

/// <summary>
/// Tests to verify middleware pipeline functionality in Requestum.
/// </summary>
public class RequestumMiddlewareTests
{
    private readonly IRequestum _requestum;

    public RequestumMiddlewareTests()
    {
        var services = new ServiceCollection();

        // Register Requestum and current assembly (containing handlers + middleware)
        services.AddRequestum(cfg =>
        {
            cfg.Default(typeof(RequestumMiddlewareTests).Assembly);
            cfg.RegisterMiddleware(typeof(TestAsyncRequestMiddleware<,>), ServiceLifetime.Singleton);
        });

        var provider = services.BuildServiceProvider();
        _requestum = provider.GetRequiredService<IRequestum>();
    }

    [Fact]
    public void CommandMiddleware_IsCalled()
    {
        // Arrange
        TestRequestMiddleware<TestCommand, CommandResponse>.Reset();

        // Act
        _requestum.Execute(new TestCommand());

        // Assert
        Assert.True(TestRequestMiddleware<TestCommand, CommandResponse>.Called, "Synchronous middleware should be called.");
    }

    [Fact]
    public async Task AsyncCommandMiddleware_IsCalled()
    {
        // Arrange
        TestAsyncRequestMiddleware<TestAsyncCommand, CommandResponse>.Reset();

        // Act
        await _requestum.ExecuteAsync(new TestAsyncCommand());

        // Assert
        Assert.True(TestAsyncRequestMiddleware<TestAsyncCommand, CommandResponse>.Called, "Asynchronous middleware should be called.");
    }

    [Fact]
    public void QueryMiddleware_IsCalled()
    {
        // Arrange
        TestRequestMiddleware<TestQuery, TestResponse>.Reset();

        // Act
        _requestum.Handle<TestQuery, TestResponse>(new TestQuery());

        // Assert
        Assert.True(TestRequestMiddleware<TestQuery, TestResponse>.Called, "Middleware should be called when handling query.");
    }

    [Fact]
    public async Task AsyncQueryMiddleware_IsCalled()
    {
        // Arrange
        TestAsyncRequestMiddleware<TestAsyncQuery, TestResponse>.Reset();

        // Act
        await _requestum.HandleAsync<TestAsyncQuery, TestResponse>(new TestAsyncQuery());

        // Assert
        Assert.True(TestAsyncRequestMiddleware<TestAsyncQuery, TestResponse>.Called, "Async middleware should be called when handling query.");
    }
}
