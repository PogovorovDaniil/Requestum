using MediatR;
using Requestum.Contract;

namespace Requestum.Benchmarks;

#region Commands and Queries
public record TestCommand(string Message) : IRequest, ICommand;
public record TestAsyncCommand(string Message) : IRequest, ICommand;
public record TestCommandWithMiddleware(string Message) : IRequest, ICommand;
public record TestAsyncCommandWithMiddleware(string Message) : IRequest, ICommand;

public record TestQuery(string Query) : IRequest<TestResponse>, IQuery<TestResponse>;
public record TestAsyncQuery(string Query) : IRequest<TestResponse>, IQuery<TestResponse>;
public record TestResponse(string Result);
#endregion

#region Tagged Commands and Queries

/// <summary>
/// Tagged command for benchmark testing.
/// </summary>
public class TaggedBenchmarkCommand : ICommand, ITaggedRequest
{
    public string[] Tags { get; }
    public string Message { get; }
    
    public TaggedBenchmarkCommand(string tag, string message)
    {
        Tags = [tag];
        Message = message;
    }
}

/// <summary>
/// Tagged async command for benchmark testing.
/// </summary>
public class TaggedAsyncBenchmarkCommand : ICommand, ITaggedRequest
{
    public string[] Tags { get; }
    public string Message { get; }
    
    public TaggedAsyncBenchmarkCommand(string tag, string message)
    {
        Tags = [tag];
        Message = message;
    }
}

/// <summary>
/// Tagged query for benchmark testing.
/// </summary>
public class TaggedBenchmarkQuery : IQuery<TestResponse>, ITaggedRequest
{
    public string[] Tags { get; }
    public string Query { get; }
    
    public TaggedBenchmarkQuery(string tag, string query)
    {
        Tags = [tag];
        Query = query;
    }
}

/// <summary>
/// Tagged async query for benchmark testing.
/// </summary>
public class TaggedAsyncBenchmarkQuery : IQuery<TestResponse>, ITaggedRequest
{
    public string[] Tags { get; }
    public string Query { get; }
    
    public TaggedAsyncBenchmarkQuery(string tag, string query)
    {
        Tags = [tag];
        Query = query;
    }
}

#endregion

#region Events
// Requestum Event Messages
public record TestEventMessage(string Message) : IEventMessage;
public record TestAsyncEventMessage(string Message) : IEventMessage;

// MediatR Notifications
public record TestNotification(string Message) : INotification;
#endregion

#region Handlers
// Combined handler for all Requestum operations
public class RequestumHandler :
    ICommandHandler<TestCommand>,
    IAsyncCommandHandler<TestAsyncCommand>,
    ICommandHandler<TestCommandWithMiddleware>,
    IAsyncCommandHandler<TestAsyncCommandWithMiddleware>,
    IQueryHandler<TestQuery, TestResponse>,
    IAsyncQueryHandler<TestAsyncQuery, TestResponse>
{
    public void Execute(TestCommand command)
    {
        _ = command.Message.Length;
    }

    public void Execute(TestCommandWithMiddleware command)
    {
        _ = command.Message.Length;
    }

    public async Task ExecuteAsync(TestAsyncCommand command, CancellationToken cancellationToken = default)
    {
        _ = command.Message.Length;
        await Task.CompletedTask;
    }

    public async Task ExecuteAsync(TestAsyncCommandWithMiddleware command, CancellationToken cancellationToken = default)
    {
        _ = command.Message.Length;
        await Task.CompletedTask;
    }

    public TestResponse Handle(TestQuery query)
    {
        _ = query.Query.Length;
        return new TestResponse("Success");
    }

    public async Task<TestResponse> HandleAsync(TestAsyncQuery query, CancellationToken cancellationToken = default)
    {
        _ = query.Query.Length;
        return await Task.FromResult(new TestResponse("Success"));
    }
}

// Combined handler for all MediatR operations
public class MediatRHandler :
    IRequestHandler<TestCommand>,
    IRequestHandler<TestAsyncCommand>,
    IRequestHandler<TestCommandWithMiddleware>,
    IRequestHandler<TestAsyncCommandWithMiddleware>,
    IRequestHandler<TestQuery, TestResponse>,
    IRequestHandler<TestAsyncQuery, TestResponse>
{
    public async Task Handle(TestCommand request, CancellationToken cancellationToken)
    {
        _ = request.Message.Length;
        await Task.CompletedTask;
    }

    public async Task Handle(TestAsyncCommand request, CancellationToken cancellationToken)
    {
        _ = request.Message.Length;
        await Task.CompletedTask;
    }

    public async Task Handle(TestCommandWithMiddleware request, CancellationToken cancellationToken)
    {
        _ = request.Message.Length;
        await Task.CompletedTask;
    }

    public async Task Handle(TestAsyncCommandWithMiddleware request, CancellationToken cancellationToken)
    {
        _ = request.Message.Length;
        await Task.CompletedTask;
    }

    public Task<TestResponse> Handle(TestQuery request, CancellationToken cancellationToken)
    {
        _ = request.Query.Length;
        return Task.FromResult(new TestResponse("Success"));
    }

    public Task<TestResponse> Handle(TestAsyncQuery request, CancellationToken cancellationToken)
    {
        _ = request.Query.Length;
        return Task.FromResult(new TestResponse("Success"));
    }
}
#endregion

#region Event Handlers
// Requestum Event Message Receivers - Single Handler
public class SingleTestEventMessageReceiver : IEventMessageReceiver<TestEventMessage>
{
    public void Receive(TestEventMessage message)
    {
        _ = message.Message.Length;
    }
}

public class SingleTestAsyncEventMessageReceiver : IAsyncEventMessageReceiver<TestAsyncEventMessage>
{
    public async Task ReceiveAsync(TestAsyncEventMessage message, CancellationToken cancellationToken = default)
    {
        _ = message.Message.Length;
        await Task.CompletedTask;
    }
}

// Requestum Event Message Receivers - Multiple Handlers (3 handlers)
public class FirstTestEventMessageReceiver : IEventMessageReceiver<TestEventMessage>
{
    public void Receive(TestEventMessage message)
    {
        _ = message.Message.Length;
    }
}

public class SecondTestEventMessageReceiver : IEventMessageReceiver<TestEventMessage>
{
    public void Receive(TestEventMessage message)
    {
        _ = message.Message.Length;
    }
}

public class ThirdTestEventMessageReceiver : IEventMessageReceiver<TestEventMessage>
{
    public void Receive(TestEventMessage message)
    {
        _ = message.Message.Length;
    }
}

public class FirstTestAsyncEventMessageReceiver : IAsyncEventMessageReceiver<TestAsyncEventMessage>
{
    public async Task ReceiveAsync(TestAsyncEventMessage message, CancellationToken cancellationToken = default)
    {
        _ = message.Message.Length;
        await Task.CompletedTask;
    }
}

public class SecondTestAsyncEventMessageReceiver : IAsyncEventMessageReceiver<TestAsyncEventMessage>
{
    public async Task ReceiveAsync(TestAsyncEventMessage message, CancellationToken cancellationToken = default)
    {
        _ = message.Message.Length;
        await Task.CompletedTask;
    }
}

public class ThirdTestAsyncEventMessageReceiver : IAsyncEventMessageReceiver<TestAsyncEventMessage>
{
    public async Task ReceiveAsync(TestAsyncEventMessage message, CancellationToken cancellationToken = default)
    {
        _ = message.Message.Length;
        await Task.CompletedTask;
    }
}

// MediatR Notification Handlers - Single Handler
public class SingleTestNotificationHandler : INotificationHandler<TestNotification>
{
    public async Task Handle(TestNotification notification, CancellationToken cancellationToken)
    {
        _ = notification.Message.Length;
        await Task.CompletedTask;
    }
}

// MediatR Notification Handlers - Multiple Handlers (3 handlers)
public class FirstTestNotificationHandler : INotificationHandler<TestNotification>
{
    public async Task Handle(TestNotification notification, CancellationToken cancellationToken)
    {
        _ = notification.Message.Length;
        await Task.CompletedTask;
    }
}

public class SecondTestNotificationHandler : INotificationHandler<TestNotification>
{
    public async Task Handle(TestNotification notification, CancellationToken cancellationToken)
    {
        _ = notification.Message.Length;
        await Task.CompletedTask;
    }
}

public class ThirdTestNotificationHandler : INotificationHandler<TestNotification>
{
    public async Task Handle(TestNotification notification, CancellationToken cancellationToken)
    {
        _ = notification.Message.Length;
        await Task.CompletedTask;
    }
}
#endregion

#region Middleware
public class TestRequestMiddleware<TRequest, TResponse> : IRequestMiddleware<TRequest, TResponse>
    where TRequest : notnull
{
    public TResponse Invoke(TRequest request, RequestNextDelegate<TRequest, TResponse> next)
    {
        _ = request.ToString();
        return next.Invoke(request);
    }
}

public class TestAsyncRequestMiddleware<TRequest, TResponse> : IAsyncRequestMiddleware<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> InvokeAsync(
        TRequest request,
        AsyncRequestNextDelegate<TRequest, TResponse> next,
        CancellationToken cancellationToken = default)
    {
        _ = request.ToString();
        return await next.InvokeAsync(request);
    }
}

#region Tagged Middleware

[MiddlewareTag("admin")]
public class BenchmarkAdminTaggedMiddleware<TRequest, TResponse> : IAsyncRequestMiddleware<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> InvokeAsync(
        TRequest request,
        AsyncRequestNextDelegate<TRequest, TResponse> next,
        CancellationToken cancellationToken = default)
    {
        _ = request.ToString();
        return await next.InvokeAsync(request);
    }
}

[MiddlewareTag("user")]
public class BenchmarkUserTaggedMiddleware<TRequest, TResponse> : IAsyncRequestMiddleware<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> InvokeAsync(
        TRequest request,
        AsyncRequestNextDelegate<TRequest, TResponse> next,
        CancellationToken cancellationToken = default)
    {
        _ = request.ToString();
        return await next.InvokeAsync(request);
    }
}

#endregion

public class TestMediatRPipelineBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        _ = request.ToString();
        return await next();
    }
}
#endregion

#region Tagged Handlers

[HandlerTag("admin")]
public class AdminTaggedBenchmarkCommandHandler : ICommandHandler<TaggedBenchmarkCommand>
{
    public void Execute(TaggedBenchmarkCommand command)
    {
        _ = command.Message.Length;
    }
}

[HandlerTag("user")]
public class UserTaggedBenchmarkCommandHandler : ICommandHandler<TaggedBenchmarkCommand>
{
    public void Execute(TaggedBenchmarkCommand command)
    {
        _ = command.Message.Length;
    }
}

// Untagged handler for TaggedBenchmarkCommand
public class UntaggedBenchmarkCommandHandler : ICommandHandler<TaggedBenchmarkCommand>
{
    public void Execute(TaggedBenchmarkCommand command)
    {
        _ = command.Message.Length;
    }
}

[HandlerTag("admin")]
public class AdminTaggedAsyncBenchmarkCommandHandler : IAsyncCommandHandler<TaggedAsyncBenchmarkCommand>
{
    public async Task ExecuteAsync(TaggedAsyncBenchmarkCommand command, CancellationToken cancellationToken = default)
    {
        _ = command.Message.Length;
        await Task.CompletedTask;
    }
}

[HandlerTag("user")]
public class UserTaggedAsyncBenchmarkCommandHandler : IAsyncCommandHandler<TaggedAsyncBenchmarkCommand>
{
    public async Task ExecuteAsync(TaggedAsyncBenchmarkCommand command, CancellationToken cancellationToken = default)
    {
        _ = command.Message.Length;
        await Task.CompletedTask;
    }
}

// Untagged handler for TaggedAsyncBenchmarkCommand
public class UntaggedAsyncBenchmarkCommandHandler : IAsyncCommandHandler<TaggedAsyncBenchmarkCommand>
{
    public async Task ExecuteAsync(TaggedAsyncBenchmarkCommand command, CancellationToken cancellationToken = default)
    {
        _ = command.Message.Length;
        await Task.CompletedTask;
    }
}

[HandlerTag("admin")]
public class AdminTaggedBenchmarkQueryHandler : IQueryHandler<TaggedBenchmarkQuery, TestResponse>
{
    public TestResponse Handle(TaggedBenchmarkQuery query)
    {
        _ = query.Query.Length;
        return new TestResponse("Success");
    }
}

[HandlerTag("user")]
public class UserTaggedBenchmarkQueryHandler : IQueryHandler<TaggedBenchmarkQuery, TestResponse>
{
    public TestResponse Handle(TaggedBenchmarkQuery query)
    {
        _ = query.Query.Length;
        return new TestResponse("Success");
    }
}

// Untagged handler for TaggedBenchmarkQuery
public class UntaggedBenchmarkQueryHandler : IQueryHandler<TaggedBenchmarkQuery, TestResponse>
{
    public TestResponse Handle(TaggedBenchmarkQuery query)
    {
        _ = query.Query.Length;
        return new TestResponse("Success");
    }
}

[HandlerTag("admin")]
public class AdminTaggedAsyncBenchmarkQueryHandler : IAsyncQueryHandler<TaggedAsyncBenchmarkQuery, TestResponse>
{
    public async Task<TestResponse> HandleAsync(TaggedAsyncBenchmarkQuery query, CancellationToken cancellationToken = default)
    {
        _ = query.Query.Length;
        return await Task.FromResult(new TestResponse("Success"));
    }
}

[HandlerTag("user")]
public class UserTaggedAsyncBenchmarkQueryHandler : IAsyncQueryHandler<TaggedAsyncBenchmarkQuery, TestResponse>
{
    public async Task<TestResponse> HandleAsync(TaggedAsyncBenchmarkQuery query, CancellationToken cancellationToken = default)
    {
        _ = query.Query.Length;
        return await Task.FromResult(new TestResponse("Success"));
    }
}

// Untagged handler for TaggedAsyncBenchmarkQuery
public class UntaggedAsyncBenchmarkQueryHandler : IAsyncQueryHandler<TaggedAsyncBenchmarkQuery, TestResponse>
{
    public async Task<TestResponse> HandleAsync(TaggedAsyncBenchmarkQuery query, CancellationToken cancellationToken = default)
    {
        _ = query.Query.Length;
        return await Task.FromResult(new TestResponse("Success"));
    }
}

#endregion