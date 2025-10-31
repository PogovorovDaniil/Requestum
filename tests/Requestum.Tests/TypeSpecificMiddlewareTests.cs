using Microsoft.Extensions.DependencyInjection;
using Requestum.Contract;

namespace Requestum.Tests;

#region Test Commands for Middleware

/// <summary>
/// Command with result for middleware testing.
/// </summary>
public record CalculateCommandWithResult(int A, int B) : ICommand<int>;

/// <summary>
/// Async command with result for middleware testing.
/// </summary>
public record FormatMessageCommandWithResult(string Text) : ICommand<string>;

#endregion

#region Command Handlers for Middleware

/// <summary>
/// Handler for CalculateCommandWithResult.
/// </summary>
public class CalculateCommandWithResultHandler : ICommandHandler<CalculateCommandWithResult, int>
{
    public int Execute(CalculateCommandWithResult command) => command.A + command.B;
}

/// <summary>
/// Handler for FormatMessageCommandWithResult.
/// </summary>
public class FormatMessageCommandWithResultHandler : IAsyncCommandHandler<FormatMessageCommandWithResult, string>
{
    public async Task<string> ExecuteAsync(FormatMessageCommandWithResult command, CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;
        return $"Formatted: {command.Text.ToUpper()}";
    }
}

#endregion

#region Type-Specific Middleware

/// <summary>
/// Middleware specifically for commands with result.
/// </summary>
public class TestCommandMiddleware<TCommand, TResponse> : IRequestMiddleware<TCommand, TResponse>
    where TCommand : ICommand<TResponse>
{
    public static bool Called { get; private set; }
    public static Type? LastCommandType { get; private set; }

    public TResponse Invoke(TCommand request, RequestNextDelegate<TCommand, TResponse> next)
    {
        Called = true;
        LastCommandType = typeof(TCommand);
        return next.Invoke(request);
    }

    public static void Reset()
    {
        Called = false;
        LastCommandType = null;
    }
}

/// <summary>
/// Middleware specifically for queries.
/// </summary>
public class TestQueryMiddleware<TQuery, TResponse> : IRequestMiddleware<TQuery, TResponse>
    where TQuery : IQuery<TResponse>
{
    public static bool Called { get; private set; }
    public static Type? LastQueryType { get; private set; }

    public TResponse Invoke(TQuery request, RequestNextDelegate<TQuery, TResponse> next)
    {
        Called = true;
        LastQueryType = typeof(TQuery);
        return next.Invoke(request);
    }

    public static void Reset()
    {
        Called = false;
        LastQueryType = null;
    }
}

/// <summary>
/// Async middleware specifically for commands with result.
/// </summary>
public class TestAsyncCommandMiddleware<TCommand, TResponse> : IAsyncRequestMiddleware<TCommand, TResponse>
    where TCommand : ICommand<TResponse>
{
    public static bool Called { get; private set; }
    public static Type? LastCommandType { get; private set; }

    public async Task<TResponse> InvokeAsync(
        TCommand request,
        AsyncRequestNextDelegate<TCommand, TResponse> next,
        CancellationToken cancellationToken = default)
    {
        Called = true;
        LastCommandType = typeof(TCommand);
        return await next.InvokeAsync(request);
    }

    public static void Reset()
    {
        Called = false;
        LastCommandType = null;
    }
}

/// <summary>
/// Async middleware specifically for queries.
/// </summary>
public class TestAsyncQueryMiddleware<TQuery, TResponse> : IAsyncRequestMiddleware<TQuery, TResponse>
    where TQuery : IQuery<TResponse>
{
    public static bool Called { get; private set; }
    public static Type? LastQueryType { get; private set; }

    public async Task<TResponse> InvokeAsync(
        TQuery request,
        AsyncRequestNextDelegate<TQuery, TResponse> next,
        CancellationToken cancellationToken = default)
    {
        Called = true;
        LastQueryType = typeof(TQuery);
        return await next.InvokeAsync(request);
    }

    public static void Reset()
    {
        Called = false;
        LastQueryType = null;
    }
}

#endregion

/// <summary>
/// Tests for type-specific middleware that should only be called for specific request types.
/// Verifies that middleware with generic constraints (ICommand&lt;TResponse&gt;, IQuery&lt;TResponse&gt;)
/// are only invoked for matching request types.
/// </summary>
public class TypeSpecificMiddlewareTests
{
    private readonly RequestumCore _requestum;

    public TypeSpecificMiddlewareTests()
    {
        var services = new ServiceCollection();

        services.AddRequestum(cfg =>
        {
            cfg.RegisterHandlers(typeof(TypeSpecificMiddlewareTests).Assembly);
            cfg.RegisterMiddlewares(typeof(TypeSpecificMiddlewareTests).Assembly);
        });

        var provider = services.BuildServiceProvider();
        _requestum = (RequestumCore)provider.GetRequiredService<IRequestum>();
    }

    #region Sync Middleware Tests

    [Fact]
    public void CommandMiddleware_CalledForCommands_NotForQueries()
    {
        // Arrange
        TestCommandMiddleware<CalculateCommandWithResult, int>.Reset();
        TestQueryMiddleware<TestQuery, TestResponse>.Reset();

        var command = new CalculateCommandWithResult(10, 20);
        var query = new TestQuery();

        // Act
        _requestum.Execute<CalculateCommandWithResult, int>(command);
        _requestum.Handle<TestQuery, TestResponse>(query);

        // Assert - Command middleware should be called only for commands
        Assert.True(
            TestCommandMiddleware<CalculateCommandWithResult, int>.Called,
            "Command middleware should be called for command.");
        Assert.Equal(
            typeof(CalculateCommandWithResult),
            TestCommandMiddleware<CalculateCommandWithResult, int>.LastCommandType);

        // Assert - Query middleware should be called only for queries
        Assert.True(
            TestQueryMiddleware<TestQuery, TestResponse>.Called,
            "Query middleware should be called for query.");
        Assert.Equal(
            typeof(TestQuery),
            TestQueryMiddleware<TestQuery, TestResponse>.LastQueryType);
    }

    [Fact]
    public void CommandMiddleware_NotCalledForQueries()
    {
        // Arrange
        TestCommandMiddleware<CalculateCommandWithResult, int>.Reset();
        var query = new TestQuery();

        // Act
        _requestum.Handle<TestQuery, TestResponse>(query);

        // Assert - Command-specific middleware should NOT be called for queries
        Assert.False(
            TestCommandMiddleware<CalculateCommandWithResult, int>.Called,
            "Command middleware should NOT be called for query.");
    }

    [Fact]
    public void QueryMiddleware_NotCalledForCommands()
    {
        // Arrange
        TestQueryMiddleware<TestQuery, TestResponse>.Reset();
        var command = new CalculateCommandWithResult(5, 10);

        // Act
        _requestum.Execute<CalculateCommandWithResult, int>(command);

        // Assert - Query-specific middleware should NOT be called for commands
        Assert.False(
            TestQueryMiddleware<TestQuery, TestResponse>.Called,
            "Query middleware should NOT be called for command.");
    }

    #endregion

    #region Async Middleware Tests

    [Fact]
    public async Task AsyncCommandMiddleware_CalledForCommands_NotForQueries()
    {
        // Arrange
        TestAsyncCommandMiddleware<FormatMessageCommandWithResult, string>.Reset();
        TestAsyncQueryMiddleware<TestAsyncQuery, TestResponse>.Reset();

        var command = new FormatMessageCommandWithResult("Test");
        var query = new TestAsyncQuery();

        // Act
        await _requestum.ExecuteAsync<FormatMessageCommandWithResult, string>(command);
        await _requestum.HandleAsync<TestAsyncQuery, TestResponse>(query);

        // Assert - Command middleware should be called only for commands
        Assert.True(
                  TestAsyncCommandMiddleware<FormatMessageCommandWithResult, string>.Called,
         "Async command middleware should be called for command.");
        Assert.Equal(
            typeof(FormatMessageCommandWithResult),
            TestAsyncCommandMiddleware<FormatMessageCommandWithResult, string>.LastCommandType);

        // Assert - Query middleware should be called only for queries
        Assert.True(
            TestAsyncQueryMiddleware<TestAsyncQuery, TestResponse>.Called,
            "Async query middleware should be called for query.");
        Assert.Equal(
            typeof(TestAsyncQuery),
            TestAsyncQueryMiddleware<TestAsyncQuery, TestResponse>.LastQueryType);
    }

    [Fact]
    public async Task AsyncCommandMiddleware_NotCalledForQueries()
    {
        // Arrange
        TestAsyncCommandMiddleware<FormatMessageCommandWithResult, string>.Reset();
        var query = new TestAsyncQuery();

        // Act
        await _requestum.HandleAsync<TestAsyncQuery, TestResponse>(query);

        // Assert - Command-specific middleware should NOT be called for queries
        Assert.False(
            TestAsyncCommandMiddleware<FormatMessageCommandWithResult, string>.Called,
            "Async command middleware should NOT be called for query.");
    }

    [Fact]
    public async Task AsyncQueryMiddleware_NotCalledForCommands()
    {
        // Arrange
        TestAsyncQueryMiddleware<TestAsyncQuery, TestResponse>.Reset();
        var command = new FormatMessageCommandWithResult("Hello");

        // Act
        await _requestum.ExecuteAsync<FormatMessageCommandWithResult, string>(command);

        // Assert - Query-specific middleware should NOT be called for commands
        Assert.False(
            TestAsyncQueryMiddleware<TestAsyncQuery, TestResponse>.Called,
            "Async query middleware should NOT be called for command.");
    }

    #endregion

    #region Multiple Request Tests

    [Fact]
    public void CommandMiddleware_TracksMultipleCommandTypes()
    {
        // Arrange
        TestCommandMiddleware<CalculateCommandWithResult, int>.Reset();
        var command = new CalculateCommandWithResult(100, 200);

        // Act
        _requestum.Execute<CalculateCommandWithResult, int>(command);

        // Assert
        Assert.True(TestCommandMiddleware<CalculateCommandWithResult, int>.Called);
        Assert.Equal(typeof(CalculateCommandWithResult), TestCommandMiddleware<CalculateCommandWithResult, int>.LastCommandType);
    }

    [Fact]
    public async Task AsyncMiddleware_TracksMultipleRequestTypes()
    {
        // Arrange
        TestAsyncCommandMiddleware<FormatMessageCommandWithResult, string>.Reset();
        TestAsyncQueryMiddleware<TestAsyncQuery, TestResponse>.Reset();

        // Act - Execute multiple commands and queries
        await _requestum.ExecuteAsync<FormatMessageCommandWithResult, string>(new FormatMessageCommandWithResult("First"));
        await _requestum.HandleAsync<TestAsyncQuery, TestResponse>(new TestAsyncQuery());
        await _requestum.ExecuteAsync<FormatMessageCommandWithResult, string>(new FormatMessageCommandWithResult("Second"));

        // Assert - Middleware should track the last call
        Assert.True(TestAsyncCommandMiddleware<FormatMessageCommandWithResult, string>.Called);
        Assert.Equal(typeof(FormatMessageCommandWithResult), TestAsyncCommandMiddleware<FormatMessageCommandWithResult, string>.LastCommandType);

        Assert.True(TestAsyncQueryMiddleware<TestAsyncQuery, TestResponse>.Called);
        Assert.Equal(typeof(TestAsyncQuery), TestAsyncQueryMiddleware<TestAsyncQuery, TestResponse>.LastQueryType);
    }

    #endregion
}
