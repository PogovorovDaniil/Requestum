using Microsoft.Extensions.DependencyInjection;
using Requestum.Contract;

namespace Requestum.Tests;

#region Test Data for Commands with Result

/// <summary>
/// Synchronous command that returns a result.
/// </summary>
public record CreateUserCommandWithResult(string Name) : ICommand<int>;

/// <summary>
/// Asynchronous command that returns a result.
/// </summary>
public record ProcessMessageCommandWithResult(string Message) : ICommand<string>;

#endregion

#region Command Handlers

/// <summary>
/// Handler for synchronous command with result.
/// </summary>
public class CreateUserCommandWithResultHandler : ICommandHandler<CreateUserCommandWithResult, int>
{
    public static bool Executed { get; private set; }
    public static string? LastName { get; private set; }

    public int Execute(CreateUserCommandWithResult command)
    {
        Executed = true;
        LastName = command.Name;
        return command.Name.Length; // Return length as user ID
    }

    public static void Reset()
    {
        Executed = false;
        LastName = null;
    }
}

/// <summary>
/// Handler for asynchronous command with result.
/// </summary>
public class ProcessMessageCommandWithResultHandler : IAsyncCommandHandler<ProcessMessageCommandWithResult, string>
{
    public static bool Executed { get; private set; }
    public static string? LastMessage { get; private set; }

    public async Task<string> ExecuteAsync(ProcessMessageCommandWithResult command, CancellationToken cancellationToken = default)
    {
        Executed = true;
        LastMessage = command.Message;
        await Task.Delay(1, cancellationToken);
        return $"Processed: {command.Message}";
    }

    public static void Reset()
    {
        Executed = false;
        LastMessage = null;
    }
}

#endregion

/// <summary>
/// Tests for commands with return values.
/// </summary>
public class CommandWithResultTests
{
    private readonly IRequestum _requestum;

    public CommandWithResultTests()
    {
        var services = new ServiceCollection();

        services.AddRequestum(cfg =>
        {
            cfg.RegisterHandlers(typeof(CommandWithResultTests).Assembly);
        });

        var provider = services.BuildServiceProvider();
        _requestum = provider.GetRequiredService<IRequestum>();
    }

    [Fact]
    public void Execute_CommandWithResult_ReturnsValue()
    {
        // Arrange
        CreateUserCommandWithResultHandler.Reset();
        var command = new CreateUserCommandWithResult("Alice");

        // Act
        var result = _requestum.Execute<CreateUserCommandWithResult, int>(command);

        // Assert
        Assert.True(CreateUserCommandWithResultHandler.Executed, "Handler should be executed.");
        Assert.Equal("Alice", CreateUserCommandWithResultHandler.LastName);
        Assert.Equal(5, result); // "Alice".Length = 5
    }

    [Fact]
    public async Task ExecuteAsync_CommandWithResult_ReturnsValue()
    {
        // Arrange
        ProcessMessageCommandWithResultHandler.Reset();
        var command = new ProcessMessageCommandWithResult("Test Message");

        // Act
        var result = await _requestum.ExecuteAsync<ProcessMessageCommandWithResult, string>(command);

        // Assert
        Assert.True(ProcessMessageCommandWithResultHandler.Executed, "Handler should be executed.");
        Assert.Equal("Test Message", ProcessMessageCommandWithResultHandler.LastMessage);
        Assert.Equal("Processed: Test Message", result);
    }

    [Fact]
    public void Execute_CommandWithResult_MultipleValues_ReturnsCorrectResults()
    {
        // Arrange
        CreateUserCommandWithResultHandler.Reset();

        // Act & Assert
        var result1 = _requestum.Execute<CreateUserCommandWithResult, int>(new CreateUserCommandWithResult("Bob"));
        Assert.Equal(3, result1); // "Bob".Length = 3

        var result2 = _requestum.Execute<CreateUserCommandWithResult, int>(new CreateUserCommandWithResult("Charlie"));
        Assert.Equal(7, result2); // "Charlie".Length = 7

        var result3 = _requestum.Execute<CreateUserCommandWithResult, int>(new CreateUserCommandWithResult(""));
        Assert.Equal(0, result3); // "".Length = 0
    }

    [Fact]
    public async Task ExecuteAsync_CommandWithResult_MultipleValues_ReturnsCorrectResults()
    {
        // Arrange
        ProcessMessageCommandWithResultHandler.Reset();

        // Act & Assert
        var result1 = await _requestum.ExecuteAsync<ProcessMessageCommandWithResult, string>(new ProcessMessageCommandWithResult("First"));
        Assert.Equal("Processed: First", result1);

        var result2 = await _requestum.ExecuteAsync<ProcessMessageCommandWithResult, string>(new ProcessMessageCommandWithResult("Second"));
        Assert.Equal("Processed: Second", result2);

        var result3 = await _requestum.ExecuteAsync<ProcessMessageCommandWithResult, string>(new ProcessMessageCommandWithResult(""));
        Assert.Equal("Processed: ", result3);
    }
}
