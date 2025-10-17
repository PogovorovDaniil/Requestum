using Microsoft.Extensions.DependencyInjection;
using Requestum.Contract;

namespace Requestum.Tests;

// Тестовые классы событий
public class TestEventMessage : IEventMessage { }
public class TestAsyncEventMessage : IEventMessage { }
public class TestEventMessageWithData : IEventMessage
{
    public string Data { get; set; } = string.Empty;
}

// Обработчики событий
public class TestEventMessageReceiver : IEventMessageReceiver<TestEventMessage>
{
    public static bool Received { get; private set; }
    public static int ReceivedCount { get; private set; }

    public void Receive(TestEventMessage message)
    {
        Received = true;
        ReceivedCount++;
    }

    public static void Reset()
    {
        Received = false;
        ReceivedCount = 0;
    }
}

public class TestAsyncEventMessageReceiver : IAsyncEventMessageReceiver<TestAsyncEventMessage>
{
    public static bool Received { get; private set; }
    public static int ReceivedCount { get; private set; }

    public Task ReceiveAsync(TestAsyncEventMessage message, CancellationToken cancellationToken = default)
    {
        Received = true;
        ReceivedCount++;
        return Task.CompletedTask;
    }

    public static void Reset()
    {
        Received = false;
        ReceivedCount = 0;
    }
}

public class FirstTestEventMessageReceiver : IEventMessageReceiver<TestEventMessageWithData>
{
    public static bool Received { get; private set; }
    public static string? ReceivedData { get; private set; }

    public void Receive(TestEventMessageWithData message)
    {
        Received = true;
        ReceivedData = message.Data;
    }

    public static void Reset()
    {
        Received = false;
        ReceivedData = null;
    }
}

public class SecondTestEventMessageReceiver : IEventMessageReceiver<TestEventMessageWithData>
{
    public static bool Received { get; private set; }
    public static string? ReceivedData { get; private set; }

    public void Receive(TestEventMessageWithData message)
    {
        Received = true;
        ReceivedData = message.Data;
    }

    public static void Reset()
    {
        Received = false;
        ReceivedData = null;
    }
}

public class FirstTestAsyncEventMessageReceiver : IAsyncEventMessageReceiver<TestEventMessageWithData>
{
    public static bool Received { get; private set; }
    public static string? ReceivedData { get; private set; }

    public Task ReceiveAsync(TestEventMessageWithData message, CancellationToken cancellationToken = default)
    {
        Received = true;
        ReceivedData = message.Data;
        return Task.CompletedTask;
    }

    public static void Reset()
    {
        Received = false;
        ReceivedData = null;
    }
}

public class SecondTestAsyncEventMessageReceiver : IAsyncEventMessageReceiver<TestEventMessageWithData>
{
    public static bool Received { get; private set; }
    public static string? ReceivedData { get; private set; }

    public Task ReceiveAsync(TestEventMessageWithData message, CancellationToken cancellationToken = default)
    {
        Received = true;
        ReceivedData = message.Data;
        return Task.CompletedTask;
    }

    public static void Reset()
    {
        Received = false;
        ReceivedData = null;
    }
}

// Основные тесты для EventMessage
public class RequestumEventMessageTests
{
    private readonly IRequestum _requestum;

    public RequestumEventMessageTests()
    {
        var services = new ServiceCollection();
        services.AddRequestum(cfg =>
        {
            cfg.RequireEventHandlers = false;
            cfg.RegisterHandlers(typeof(RequestumEventMessageTests).Assembly);
        });

        var provider = services.BuildServiceProvider();
        _requestum = provider.GetRequiredService<IRequestum>();
    }

    [Fact]
    public void Publish_EventMessage_CallsReceiver()
    {
        // Arrange
        TestEventMessageReceiver.Reset();
        var eventMessage = new TestEventMessage();

        // Act
        _requestum.Publish(eventMessage);

        // Assert
        Assert.True(TestEventMessageReceiver.Received);
        Assert.Equal(1, TestEventMessageReceiver.ReceivedCount);
    }

    [Fact]
    public async Task PublishAsync_AsyncEventMessage_CallsReceiver()
    {
        // Arrange
        TestAsyncEventMessageReceiver.Reset();
        var message = new TestAsyncEventMessage();

        // Act
        await _requestum.PublishAsync(message);

        // Assert
        Assert.True(TestAsyncEventMessageReceiver.Received);
        Assert.Equal(1, TestAsyncEventMessageReceiver.ReceivedCount);
    }

    [Fact]
    public void Publish_EventMessageWithData_CallsReceiver()
    {
        // Arrange
        FirstTestEventMessageReceiver.Reset();
        SecondTestEventMessageReceiver.Reset();
        var message = new TestEventMessageWithData { Data = "Test Data" };

        // Act
        _requestum.Publish(message);

        // Assert
        Assert.True(FirstTestEventMessageReceiver.Received);
        Assert.Equal("Test Data", FirstTestEventMessageReceiver.ReceivedData);
        Assert.True(SecondTestEventMessageReceiver.Received);
        Assert.Equal("Test Data", SecondTestEventMessageReceiver.ReceivedData);
    }

    [Fact]
    public async Task PublishAsync_EventMessageWithData_CallsAllReceivers()
    {
        // Arrange
        FirstTestAsyncEventMessageReceiver.Reset();
        SecondTestAsyncEventMessageReceiver.Reset();
        var message = new TestEventMessageWithData { Data = "Async Test Data" };

        // Act
        await _requestum.PublishAsync(message);

        // Assert
        Assert.True(FirstTestAsyncEventMessageReceiver.Received);
        Assert.Equal("Async Test Data", FirstTestAsyncEventMessageReceiver.ReceivedData);
        Assert.True(SecondTestAsyncEventMessageReceiver.Received);
        Assert.Equal("Async Test Data", SecondTestAsyncEventMessageReceiver.ReceivedData);
    }

    [Fact]
    public void Publish_UnregisteredEventMessage_DoesNotThrow()
    {
        // Arrange
        var message = new UnregisteredTestEventMessage();

        // Act & Assert - should not throw, just do nothing
        var exception = Record.Exception(() => _requestum.Publish(message));
        Assert.Null(exception);
    }

    [Fact]
    public async Task PublishAsync_UnregisteredEventMessage_DoesNotThrow()
    {
        // Arrange
        var message = new UnregisteredTestEventMessage();

        // Act & Assert - should not throw, just do nothing
        var exception = await Record.ExceptionAsync(() => _requestum.PublishAsync(message));
        Assert.Null(exception);
    }

    [Fact]
    public void Publish_MultipleTimesToSameReceiver_IncrementsCount()
    {
        // Arrange
        TestEventMessageReceiver.Reset();
        var message1 = new TestEventMessage();
        var message2 = new TestEventMessage();

        // Act
        _requestum.Publish(message1);
        _requestum.Publish(message2);

        // Assert
        Assert.True(TestEventMessageReceiver.Received);
        Assert.Equal(2, TestEventMessageReceiver.ReceivedCount);
    }

    [Fact]
    public async Task PublishAsync_MultipleTimesToSameReceiver_IncrementsCount()
    {
        // Arrange
        TestAsyncEventMessageReceiver.Reset();
        var message1 = new TestAsyncEventMessage();
        var message2 = new TestAsyncEventMessage();

        // Act
        await _requestum.PublishAsync(message1);
        await _requestum.PublishAsync(message2);

        // Assert
        Assert.True(TestAsyncEventMessageReceiver.Received);
        Assert.Equal(2, TestAsyncEventMessageReceiver.ReceivedCount);
    }

    #region Parameterless Publish Tests

    [Fact]
    public void Publish_ParameterlessEventMessage_CallsReceiver()
    {
        // Arrange
        TestEventMessageReceiver.Reset();

        // Act
        _requestum.Publish<TestEventMessage>();

        // Assert
        Assert.True(TestEventMessageReceiver.Received);
        Assert.Equal(1, TestEventMessageReceiver.ReceivedCount);
    }

    [Fact]
    public async Task PublishAsync_ParameterlessEventMessage_CallsReceiver()
    {
        // Arrange
        TestAsyncEventMessageReceiver.Reset();

        // Act
        await _requestum.PublishAsync<TestAsyncEventMessage>();

        // Assert
        Assert.True(TestAsyncEventMessageReceiver.Received);
        Assert.Equal(1, TestAsyncEventMessageReceiver.ReceivedCount);
    }

    [Fact]
    public void Publish_ParameterlessEventMessage_UsesCachedInstance()
    {
        // Arrange
        TestEventMessageReceiver.Reset();

        // Act - вызываем дважды
        _requestum.Publish<TestEventMessage>();
        TestEventMessageReceiver.Reset();
        _requestum.Publish<TestEventMessage>();

        // Assert - второй вызов также должен работать с закэшированным экземпляром
        Assert.True(TestEventMessageReceiver.Received);
        Assert.Equal(1, TestEventMessageReceiver.ReceivedCount);
    }

    [Fact]
    public async Task PublishAsync_ParameterlessEventMessage_UsesCachedInstance()
    {
        // Arrange
        TestAsyncEventMessageReceiver.Reset();

        // Act - вызываем дважды
        await _requestum.PublishAsync<TestAsyncEventMessage>();
        TestAsyncEventMessageReceiver.Reset();
        await _requestum.PublishAsync<TestAsyncEventMessage>();

        // Assert - второй вызов также должен работать с закэшированным экземпляром
        Assert.True(TestAsyncEventMessageReceiver.Received);
        Assert.Equal(1, TestAsyncEventMessageReceiver.ReceivedCount);
    }

    [Fact]
    public void Publish_ParameterlessUnregisteredEventMessage_DoesNotThrow()
    {
        // Act & Assert - should not throw when RequireEventHandlers is false
        var exception = Record.Exception(() => _requestum.Publish<UnregisteredTestEventMessage>());
        Assert.Null(exception);
    }

    [Fact]
    public async Task PublishAsync_ParameterlessUnregisteredEventMessage_DoesNotThrow()
    {
        // Act & Assert - should not throw when RequireEventHandlers is false
        var exception = await Record.ExceptionAsync(() => _requestum.PublishAsync<UnregisteredTestEventMessage>());
        Assert.Null(exception);
    }

    #endregion
}

public class UnregisteredTestEventMessage : IEventMessage { }
