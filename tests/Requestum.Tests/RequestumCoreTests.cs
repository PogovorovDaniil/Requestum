using Microsoft.Extensions.DependencyInjection;
using Requestum.Contract;

namespace Requestum.Tests;

// Простые тестовые типы
public class TestCommand : ICommand { }
public class TestAsyncCommand : ICommand { }

public class TestQuery : IQuery<TestResponse> { }
public class TestAsyncQuery : IQuery<TestResponse> { }

public class UnregisteredTestCommand : ICommand { }
public class UnregisteredTestQuery : IQuery<TestResponse> { }

public class TestResponse { }

// Обработчики
public class TestCommandHandler : ICommandHandler<TestCommand>
{
    public static bool Executed { get; private set; }

    public void Execute(TestCommand command) => Executed = true;

    public static void Reset() => Executed = false;
}

public class TestAsyncCommandHandler : IAsyncCommandHandler<TestAsyncCommand>
{
    public static bool Executed { get; private set; }

    public Task ExecuteAsync(TestAsyncCommand command, CancellationToken cancellationToken = default)
    {
        Executed = true;
        return Task.CompletedTask;
    }

    public static void Reset() => Executed = false;
}

public class TestQueryHandler : IQueryHandler<TestQuery, TestResponse>
{
    public TestResponse Handle(TestQuery query) => new();
}

public class TestAsyncQueryHandler : IAsyncQueryHandler<TestAsyncQuery, TestResponse>
{
    public Task<TestResponse> HandleAsync(TestAsyncQuery query, CancellationToken cancellationToken = default)
        => Task.FromResult(new TestResponse());
}

// Основные тесты ядра Requestum
public class RequestumCoreTests
{
    private readonly IRequestum _requestum;

    public RequestumCoreTests()
    {
        var services = new ServiceCollection();
        services.AddRequestum(cfg => cfg.RegisterHandlers(typeof(RequestumCoreTests).Assembly));

        var provider = services.BuildServiceProvider();
        _requestum = provider.GetRequiredService<IRequestum>();
    }

    [Fact]
    public void Execute_Command_CallsHandler()
    {
        // Arrange
        TestCommandHandler.Reset();
        var command = new TestCommand();

        // Act
        _requestum.Execute(command);

        // Assert
        Assert.True(TestCommandHandler.Executed);
    }

    [Fact]
    public async Task ExecuteAsync_AsyncCommand_CallsHandler()
    {
        // Arrange
        TestAsyncCommandHandler.Reset();
        var command = new TestAsyncCommand();

        // Act
        await _requestum.ExecuteAsync(command);

        // Assert
        Assert.True(TestAsyncCommandHandler.Executed);
    }

    [Fact]
    public void Handle_Query_ReturnsResponse()
    {
        // Arrange
        var query = new TestQuery();

        // Act
        var response = _requestum.Handle<TestQuery, TestResponse>(query);

        // Assert
        Assert.NotNull(response);
        Assert.IsType<TestResponse>(response);
    }

    [Fact]
    public async Task HandleAsync_AsyncQuery_ReturnsResponse()
    {
        // Arrange
        var query = new TestAsyncQuery();

        // Act
        var response = await _requestum.HandleAsync<TestAsyncQuery, TestResponse>(query);

        // Assert
        Assert.NotNull(response);
        Assert.IsType<TestResponse>(response);
    }

    [Fact]
    public void Execute_UnregisteredCommand_Throws()
    {
        // Arrange
        var command = new UnregisteredTestCommand();

        // Act & Assert
        Assert.Throws<RequestumException>(() => _requestum.Execute(command));
    }

    [Fact]
    public void Handle_UnregisteredQuery_Throws()
    {
        // Arrange
        var query = new UnregisteredTestQuery();

        // Act & Assert
        Assert.Throws<RequestumException>(() => _requestum.Handle<UnregisteredTestQuery, TestResponse>(query));
    }

    #region Parameterless Tests

    [Fact]
    public void Execute_ParameterlessCommand_CallsHandler()
    {
        // Arrange
        TestCommandHandler.Reset();

        // Act
        _requestum.Execute<TestCommand>();

        // Assert
        Assert.True(TestCommandHandler.Executed);
    }

    [Fact]
    public async Task ExecuteAsync_ParameterlessCommand_CallsHandler()
    {
        // Arrange
        TestAsyncCommandHandler.Reset();

        // Act
        await _requestum.ExecuteAsync<TestAsyncCommand>();

        // Assert
        Assert.True(TestAsyncCommandHandler.Executed);
    }

    [Fact]
    public void Handle_ParameterlessQuery_ReturnsResponse()
    {
        // Act
        var response = _requestum.Handle<TestQuery, TestResponse>();

        // Assert
        Assert.NotNull(response);
        Assert.IsType<TestResponse>(response);
    }

    [Fact]
    public async Task HandleAsync_ParameterlessQuery_ReturnsResponse()
    {
        // Act
        var response = await _requestum.HandleAsync<TestQuery, TestResponse>();

        // Assert
        Assert.NotNull(response);
        Assert.IsType<TestResponse>(response);
    }

    [Fact]
    public void Execute_ParameterlessCommand_UsesCachedInstance()
    {
        // Arrange
        TestCommandHandler.Reset();

        // Act - вызываем дважды
        _requestum.Execute<TestCommand>();
        TestCommandHandler.Reset();
        _requestum.Execute<TestCommand>();

        // Assert - второй вызов также должен работать с закэшированным экземпляром
        Assert.True(TestCommandHandler.Executed);
    }

    [Fact]
    public async Task ExecuteAsync_ParameterlessCommand_UsesCachedInstance()
    {
        // Arrange
        TestAsyncCommandHandler.Reset();

        // Act - вызываем дважды
        await _requestum.ExecuteAsync<TestAsyncCommand>();
        TestAsyncCommandHandler.Reset();
        await _requestum.ExecuteAsync<TestAsyncCommand>();

        // Assert - второй вызов также должен работать с закэшированным экземпляром
        Assert.True(TestAsyncCommandHandler.Executed);
    }

    [Fact]
    public void Execute_ParameterlessUnregisteredCommand_Throws()
    {
        // Act & Assert
        Assert.Throws<RequestumException>(() => _requestum.Execute<UnregisteredTestCommand>());
    }

    [Fact]
    public void Handle_ParameterlessUnregisteredQuery_Throws()
    {
        // Act & Assert
        Assert.Throws<RequestumException>(() => _requestum.Handle<UnregisteredTestQuery, TestResponse>());
    }

    #endregion
}
