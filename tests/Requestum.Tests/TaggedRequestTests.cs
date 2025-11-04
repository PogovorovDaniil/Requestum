using Microsoft.Extensions.DependencyInjection;
using Requestum.Contract;

namespace Requestum.Tests;

#region Test Commands and Queries with Tags

public class TaggedTestCommand : ICommand, ITaggedRequest
{
    public string Tag { get; }
    public TaggedTestCommand(string tag) => Tag = tag;
}

public class TaggedTestQuery : IQuery<TestResponse>, ITaggedRequest
{
    public string Tag { get; }
    public TaggedTestQuery(string tag) => Tag = tag;
}

public class TaggedEventMessage : IEventMessage, ITaggedRequest
{
    public string Tag { get; }
    public TaggedEventMessage(string tag) => Tag = tag;
}

#endregion

#region Handlers with Tags

[HandlerTag("admin")]
public class AdminTaggedCommandHandler : ICommandHandler<TaggedTestCommand>
{
    public static bool Executed { get; private set; }
    public void Execute(TaggedTestCommand command) => Executed = true;
    public static void Reset() => Executed = false;
}

[HandlerTag("user")]
public class UserTaggedCommandHandler : ICommandHandler<TaggedTestCommand>
{
    public static bool Executed { get; private set; }
    public void Execute(TaggedTestCommand command) => Executed = true;
    public static void Reset() => Executed = false;
}

[HandlerTag("admin")]
public class AdminTaggedQueryHandler : IQueryHandler<TaggedTestQuery, TestResponse>
{
    public static bool Executed { get; private set; }
    public TestResponse Handle(TaggedTestQuery query)
    {
        Executed = true;
        return new TestResponse();
    }
    public static void Reset() => Executed = false;
}

[HandlerTag("user")]
public class UserTaggedQueryHandler : IQueryHandler<TaggedTestQuery, TestResponse>
{
    public static bool Executed { get; private set; }
    public TestResponse Handle(TaggedTestQuery query)
    {
        Executed = true;
        return new TestResponse();
    }
    public static void Reset() => Executed = false;
}

[HandlerTag("admin")]
public class AdminTaggedEventReceiver : IEventMessageReceiver<TaggedEventMessage>
{
    public static int Count { get; private set; }
    public void Receive(TaggedEventMessage message) => Count++;
    public static void Reset() => Count = 0;
}

[HandlerTag("user")]
public class UserTaggedEventReceiver : IEventMessageReceiver<TaggedEventMessage>
{
    public static int Count { get; private set; }
    public void Receive(TaggedEventMessage message) => Count++;
    public static void Reset() => Count = 0;
}

#endregion

/// <summary>
/// Tests for tagged request functionality.
/// Key behaviors:
/// - Commands/Queries: Only ONE handler should match. Multiple matches throw exception.
/// - Events: Multiple receivers are allowed and all matching receivers execute.
/// - Tag matching: Handler matches if ANY of its tags appear in request's tags.
/// </summary>
public class TaggedRequestTests
{
    private readonly IRequestum _requestum;

    public TaggedRequestTests()
    {
        var services = new ServiceCollection();
        services.AddRequestum(cfg =>
        {
            cfg.RequireEventHandlers = false;
            cfg.RegisterHandlers(typeof(TaggedRequestTests).Assembly);
        });
        _requestum = services.BuildServiceProvider().GetRequiredService<IRequestum>();
    }

    #region Command Tests

    [Fact]
    public void Execute_TaggedCommand_WithAdminTag_ExecutesAdminHandler()
    {
        // Arrange
        AdminTaggedCommandHandler.Reset();
        UserTaggedCommandHandler.Reset();
        var command = new TaggedTestCommand("admin");

        // Act
        _requestum.Execute(command);

        // Assert
        Assert.True(AdminTaggedCommandHandler.Executed);
        Assert.False(UserTaggedCommandHandler.Executed);
    }

    [Fact]
    public void Execute_TaggedCommand_WithUserTag_ExecutesUserHandler()
    {
        // Arrange
        AdminTaggedCommandHandler.Reset();
        UserTaggedCommandHandler.Reset();
        var command = new TaggedTestCommand("user");

        // Act
        _requestum.Execute(command);

        // Assert
        Assert.False(AdminTaggedCommandHandler.Executed);
        Assert.True(UserTaggedCommandHandler.Executed);
    }

    [Fact]
    public void Execute_TaggedCommand_WithUnknownTag_ThrowsException()
    {
        // Arrange
        AdminTaggedCommandHandler.Reset();
        UserTaggedCommandHandler.Reset();
        var command = new TaggedTestCommand("guest");

        // Act & Assert
        Assert.Throws<RequestumException>(() => _requestum.Execute(command));
        Assert.False(AdminTaggedCommandHandler.Executed);
        Assert.False(UserTaggedCommandHandler.Executed);
    }

    #endregion

    #region Query Tests

    [Fact]
    public void Handle_TaggedQuery_WithAdminTag_ExecutesAdminHandler()
    {
        // Arrange
        AdminTaggedQueryHandler.Reset();
        UserTaggedQueryHandler.Reset();
        var query = new TaggedTestQuery("admin");

        // Act
        var result = _requestum.Handle<TaggedTestQuery, TestResponse>(query);

        // Assert
        Assert.NotNull(result);
        Assert.True(AdminTaggedQueryHandler.Executed);
        Assert.False(UserTaggedQueryHandler.Executed);
    }

    [Fact]
    public void Handle_TaggedQuery_WithUserTag_ExecutesUserHandler()
    {
        // Arrange
        AdminTaggedQueryHandler.Reset();
        UserTaggedQueryHandler.Reset();
        var query = new TaggedTestQuery("user");

        // Act
        var result = _requestum.Handle<TaggedTestQuery, TestResponse>(query);

        // Assert
        Assert.NotNull(result);
        Assert.False(AdminTaggedQueryHandler.Executed);
        Assert.True(UserTaggedQueryHandler.Executed);
    }

    [Fact]
    public void Handle_TaggedQuery_WithUnknownTag_ThrowsException()
    {
        // Arrange
        AdminTaggedQueryHandler.Reset();
        UserTaggedQueryHandler.Reset();
        var query = new TaggedTestQuery("guest");

        // Act & Assert
        Assert.Throws<RequestumException>(() => _requestum.Handle<TaggedTestQuery, TestResponse>(query));
        Assert.False(AdminTaggedQueryHandler.Executed);
        Assert.False(UserTaggedQueryHandler.Executed);
    }

    #endregion

    #region Event Message Tests

    [Fact]
    public void Publish_TaggedEventMessage_WithAdminTag_ExecutesOnlyAdminReceiver()
    {
        // Arrange
        AdminTaggedEventReceiver.Reset();
        UserTaggedEventReceiver.Reset();
        var message = new TaggedEventMessage("admin");

        // Act
        _requestum.Publish(message);

        // Assert
        Assert.Equal(1, AdminTaggedEventReceiver.Count);
        Assert.Equal(0, UserTaggedEventReceiver.Count);
    }

    [Fact]
    public void Publish_TaggedEventMessage_WithUserTag_ExecutesOnlyUserReceiver()
    {
        // Arrange
        AdminTaggedEventReceiver.Reset();
        UserTaggedEventReceiver.Reset();
        var message = new TaggedEventMessage("user");

        // Act
        _requestum.Publish(message);

        // Assert
        Assert.Equal(0, AdminTaggedEventReceiver.Count);
        Assert.Equal(1, UserTaggedEventReceiver.Count);
    }

    [Fact]
    public void Publish_TaggedEventMessage_WithUnknownTag_DoesNotExecuteAnyReceiver()
    {
        // Arrange
        AdminTaggedEventReceiver.Reset();
        UserTaggedEventReceiver.Reset();
        var message = new TaggedEventMessage("guest");

        // Act
        _requestum.Publish(message);

        // Assert
        Assert.Equal(0, AdminTaggedEventReceiver.Count);
        Assert.Equal(0, UserTaggedEventReceiver.Count);
    }

    [Fact]
    public void Publish_TaggedEventMessage_MultipleTimes_IncrementsCountForMatchingTag()
    {
        // Arrange
        AdminTaggedEventReceiver.Reset();
        UserTaggedEventReceiver.Reset();
        var message1 = new TaggedEventMessage("admin");
        var message2 = new TaggedEventMessage("admin");

        // Act
        _requestum.Publish(message1);
        _requestum.Publish(message2);

        // Assert
        Assert.Equal(2, AdminTaggedEventReceiver.Count);
        Assert.Equal(0, UserTaggedEventReceiver.Count);
    }

    #endregion

    #region Tag Isolation Tests

    [Fact]
    public void Execute_DifferentTags_IsolatesHandlerExecution()
    {
        // Arrange
        AdminTaggedCommandHandler.Reset();
        UserTaggedCommandHandler.Reset();
        var adminCommand = new TaggedTestCommand("admin");
        var userCommand = new TaggedTestCommand("user");

        // Act
        _requestum.Execute(adminCommand);
        AdminTaggedCommandHandler.Reset();
        _requestum.Execute(userCommand);

        // Assert
        Assert.False(AdminTaggedCommandHandler.Executed);
        Assert.True(UserTaggedCommandHandler.Executed);
    }

    [Fact]
    public void Handle_DifferentTags_IsolatesHandlerExecution()
    {
        // Arrange
        AdminTaggedQueryHandler.Reset();
        UserTaggedQueryHandler.Reset();
        var adminQuery = new TaggedTestQuery("admin");
        var userQuery = new TaggedTestQuery("user");

        // Act
        _requestum.Handle<TaggedTestQuery, TestResponse>(adminQuery);
        AdminTaggedQueryHandler.Reset();
        var result = _requestum.Handle<TaggedTestQuery, TestResponse>(userQuery);

        // Assert
        Assert.NotNull(result);
        Assert.False(AdminTaggedQueryHandler.Executed);
        Assert.True(UserTaggedQueryHandler.Executed);
    }

    [Fact]
    public void Publish_DifferentTags_IsolatesReceiverExecution()
    {
        // Arrange
        AdminTaggedEventReceiver.Reset();
        UserTaggedEventReceiver.Reset();
        var adminMessage = new TaggedEventMessage("admin");
        var userMessage = new TaggedEventMessage("user");

        // Act
        _requestum.Publish(adminMessage);
        _requestum.Publish(userMessage);

        // Assert
        Assert.Equal(1, AdminTaggedEventReceiver.Count);
        Assert.Equal(1, UserTaggedEventReceiver.Count);
    }

    #endregion
}
