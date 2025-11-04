using Microsoft.Extensions.DependencyInjection;
using Requestum.Contract;

namespace Requestum.Tests;

#region Test Commands and Queries

public class GlobalTagTestCommand : ICommand
{
    public string Message { get; set; } = string.Empty;
}

public class GlobalTagTestQuery : IQuery<TestResponse>
{
    public string Query { get; set; } = string.Empty;
}

public class GlobalTagTestEventMessage : IEventMessage
{
    public string Message { get; set; } = string.Empty;
}

#endregion

#region Handlers with Global Tags

[HandlerTag("global")]
public class GlobalTagCommandHandler : ICommandHandler<GlobalTagTestCommand>
{
    public static bool Executed { get; private set; }
    public void Execute(GlobalTagTestCommand command) => Executed = true;
    public static void Reset() => Executed = false;
}

[HandlerTag("global")]
public class GlobalTagQueryHandler : IQueryHandler<GlobalTagTestQuery, TestResponse>
{
    public static bool Executed { get; private set; }
    public TestResponse Handle(GlobalTagTestQuery query)
    {
        Executed = true;
        return new TestResponse();
    }
    public static void Reset() => Executed = false;
}

[HandlerTag("global")]
public class GlobalTagEventReceiver : IEventMessageReceiver<GlobalTagTestEventMessage>
{
    public static int Count { get; private set; }
    public void Receive(GlobalTagTestEventMessage message) => Count++;
    public static void Reset() => Count = 0;
}

public class UntaggedGlobalTestCommandHandler : ICommandHandler<GlobalTagTestCommand>
{
    public static bool Executed { get; private set; }
    public void Execute(GlobalTagTestCommand command) => Executed = true;
    public static void Reset() => Executed = false;
}

#endregion

#region Middlewares with Global Tags

[MiddlewareTag("global")]
public class GlobalTagMiddleware<TRequest, TResponse> : IRequestMiddleware<TRequest, TResponse>
{
    public static bool Called { get; private set; }
    public TResponse Invoke(TRequest request, RequestNextDelegate<TRequest, TResponse> next)
    {
        Called = true;
        return next.Invoke(request);
    }
    public static void Reset() => Called = false;
}

public class UntaggedGlobalTestMiddleware<TRequest, TResponse> : IRequestMiddleware<TRequest, TResponse>
{
    public static bool Called { get; private set; }
    public TResponse Invoke(TRequest request, RequestNextDelegate<TRequest, TResponse> next)
    {
        Called = true;
        return next.Invoke(request);
    }
    public static void Reset() => Called = false;
}

#endregion

/// <summary>
/// Тесты для проверки функциональности GlobalTags.
/// GlobalTags - это массив тегов, который применяется глобально ко всем запросам,
/// позволяя подключать middleware и handlers с соответствующими тегами.
/// </summary>
public class GlobalTagsTests
{
    [Fact]
    public void Execute_Command_WithGlobalTag_ExecutesTaggedHandler()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddRequestum(cfg =>
        {
            cfg.GlobalTags = ["global"];
            cfg.RegisterHandlers(typeof(GlobalTagsTests).Assembly);
        });
        var requestum = services.BuildServiceProvider().GetRequiredService<IRequestum>();
        
        GlobalTagCommandHandler.Reset();
        UntaggedGlobalTestCommandHandler.Reset();
        var command = new GlobalTagTestCommand { Message = "test" };

        // Act
        requestum.Execute(command);

        // Assert
        Assert.True(GlobalTagCommandHandler.Executed, "Handler with 'global' tag should be executed");
        Assert.False(UntaggedGlobalTestCommandHandler.Executed, "Untagged handler should NOT be executed when tagged handler is found");
    }

    [Fact]
    public void Handle_Query_WithGlobalTag_ExecutesTaggedHandler()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddRequestum(cfg =>
        {
            cfg.GlobalTags = ["global"];
            cfg.RegisterHandlers(typeof(GlobalTagsTests).Assembly);
        });
        var requestum = services.BuildServiceProvider().GetRequiredService<IRequestum>();
        
        GlobalTagQueryHandler.Reset();
        var query = new GlobalTagTestQuery { Query = "test" };

        // Act
        var result = requestum.Handle<GlobalTagTestQuery, TestResponse>(query);

        // Assert
        Assert.NotNull(result);
        Assert.True(GlobalTagQueryHandler.Executed, "Handler with 'global' tag should be executed");
    }

    [Fact]
    public void Publish_EventMessage_WithGlobalTag_ExecutesTaggedReceiver()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddRequestum(cfg =>
        {
            cfg.GlobalTags = ["global"];
            cfg.RequireEventHandlers = false;
            cfg.RegisterHandlers(typeof(GlobalTagsTests).Assembly);
        });
        var requestum = services.BuildServiceProvider().GetRequiredService<IRequestum>();
        
        GlobalTagEventReceiver.Reset();
        var eventMessage = new GlobalTagTestEventMessage { Message = "test" };

        // Act
        requestum.Publish(eventMessage);

        // Assert
        Assert.Equal(1, GlobalTagEventReceiver.Count);
    }

    [Fact]
    public void Execute_Command_WithGlobalTag_CallsTaggedMiddleware()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddRequestum(cfg =>
        {
            cfg.GlobalTags = ["global"];
            cfg.RegisterHandlers(typeof(GlobalTagsTests).Assembly);
            cfg.RegisterMiddleware(typeof(GlobalTagMiddleware<,>), ServiceLifetime.Singleton);
            cfg.RegisterMiddleware(typeof(UntaggedGlobalTestMiddleware<,>), ServiceLifetime.Singleton);
        });
        var requestum = services.BuildServiceProvider().GetRequiredService<IRequestum>();
        
        GlobalTagMiddleware<GlobalTagTestCommand, EmptyResponse>.Reset();
        UntaggedGlobalTestMiddleware<GlobalTagTestCommand, EmptyResponse>.Reset();
        var command = new GlobalTagTestCommand { Message = "test" };

        // Act
        requestum.Execute(command);

        // Assert
        Assert.True(GlobalTagMiddleware<GlobalTagTestCommand, EmptyResponse>.Called, "Middleware with 'global' tag should be called");
        Assert.True(UntaggedGlobalTestMiddleware<GlobalTagTestCommand, EmptyResponse>.Called, "Untagged middleware should also be called");
    }

    [Fact]
    public void Handle_Query_WithGlobalTag_CallsTaggedMiddleware()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddRequestum(cfg =>
        {
            cfg.GlobalTags = ["global"];
            cfg.RegisterHandlers(typeof(GlobalTagsTests).Assembly);
            cfg.RegisterMiddleware(typeof(GlobalTagMiddleware<,>), ServiceLifetime.Singleton);
            cfg.RegisterMiddleware(typeof(UntaggedGlobalTestMiddleware<,>), ServiceLifetime.Singleton);
        });
        var requestum = services.BuildServiceProvider().GetRequiredService<IRequestum>();
        
        GlobalTagMiddleware<GlobalTagTestQuery, TestResponse>.Reset();
        UntaggedGlobalTestMiddleware<GlobalTagTestQuery, TestResponse>.Reset();
        var query = new GlobalTagTestQuery { Query = "test" };

        // Act
        var result = requestum.Handle<GlobalTagTestQuery, TestResponse>(query);

        // Assert
        Assert.NotNull(result);
        Assert.True(GlobalTagMiddleware<GlobalTagTestQuery, TestResponse>.Called, "Middleware with 'global' tag should be called");
        Assert.True(UntaggedGlobalTestMiddleware<GlobalTagTestQuery, TestResponse>.Called, "Untagged middleware should also be called");
    }

    [Fact]
    public void Execute_Command_WithMultipleGlobalTags_CallsAllTaggedMiddlewares()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddRequestum(cfg =>
        {
            cfg.GlobalTags = ["global", "monitoring"];
            cfg.RegisterHandlers(typeof(GlobalTagsTests).Assembly);
            cfg.RegisterMiddleware(typeof(GlobalTagMiddleware<,>), ServiceLifetime.Singleton);
            cfg.RegisterMiddleware(typeof(MonitoringTagMiddleware<,>), ServiceLifetime.Singleton);
        });
        var requestum = services.BuildServiceProvider().GetRequiredService<IRequestum>();
        
        GlobalTagMiddleware<GlobalTagTestCommand, EmptyResponse>.Reset();
        MonitoringTagMiddleware<GlobalTagTestCommand, EmptyResponse>.Reset();
        var command = new GlobalTagTestCommand { Message = "test" };

        // Act
        requestum.Execute(command);

        // Assert
        Assert.True(GlobalTagMiddleware<GlobalTagTestCommand, EmptyResponse>.Called, "Middleware with 'global' tag should be called");
        Assert.True(MonitoringTagMiddleware<GlobalTagTestCommand, EmptyResponse>.Called, "Middleware with 'monitoring' tag should be called");
    }

    [Fact]
    public void Execute_Command_WithNoGlobalTags_UsesUntaggedHandler()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddRequestum(cfg =>
        {
            cfg.GlobalTags = []; // No global tags
            cfg.RegisterHandlers(typeof(GlobalTagsTests).Assembly);
        });
        var requestum = services.BuildServiceProvider().GetRequiredService<IRequestum>();
        
        GlobalTagCommandHandler.Reset();
        UntaggedGlobalTestCommandHandler.Reset();
        var command = new GlobalTagTestCommand { Message = "test" };

        // Act
        requestum.Execute(command);

        // Assert
        Assert.False(GlobalTagCommandHandler.Executed, "Tagged handler should NOT be executed without global tags");
        Assert.True(UntaggedGlobalTestCommandHandler.Executed, "Untagged handler should be executed");
    }

    [Fact]
    public void Publish_EventMessage_WithMultipleGlobalTags_ExecutesAllMatchingReceivers()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddRequestum(cfg =>
        {
            cfg.GlobalTags = ["global", "monitoring"];
            cfg.RequireEventHandlers = false;
            cfg.RegisterHandlers(typeof(GlobalTagsTests).Assembly);
        });
        var requestum = services.BuildServiceProvider().GetRequiredService<IRequestum>();
        
        GlobalTagEventReceiver.Reset();
        MonitoringTagEventReceiver.Reset();
        var eventMessage = new GlobalTagTestEventMessage { Message = "test" };

        // Act
        requestum.Publish(eventMessage);

        // Assert
        Assert.Equal(1, GlobalTagEventReceiver.Count);
        Assert.Equal(1, MonitoringTagEventReceiver.Count);
    }
}

#region Additional Test Components for Multiple Tags

[MiddlewareTag("monitoring")]
public class MonitoringTagMiddleware<TRequest, TResponse> : IRequestMiddleware<TRequest, TResponse>
{
    public static bool Called { get; private set; }
    public TResponse Invoke(TRequest request, RequestNextDelegate<TRequest, TResponse> next)
    {
        Called = true;
        return next.Invoke(request);
    }
    public static void Reset() => Called = false;
}

[HandlerTag("monitoring")]
public class MonitoringTagEventReceiver : IEventMessageReceiver<GlobalTagTestEventMessage>
{
    public static int Count { get; private set; }
    public void Receive(GlobalTagTestEventMessage message) => Count++;
    public static void Reset() => Count = 0;
}

#endregion
