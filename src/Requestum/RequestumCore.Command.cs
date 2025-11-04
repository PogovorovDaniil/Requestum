using Requestum.Contract;
using Requestum.Middleware;

namespace Requestum;

public partial class RequestumCore
{
    public void Execute<TCommand>(TCommand command)
        where TCommand : ICommand
    {
        var handler = GetHandler(command);
        switch (handler)
        {
            case IAsyncCommandHandler<TCommand> asyncCommandHandler:
                BuildCommandMiddleware(new CommandMiddlewareAsyncDelegate<TCommand>(asyncCommandHandler), command)
                    .Invoke(command)
                    .Wait();
                return;
            case ICommandHandler<TCommand> commandHandler:
                BuildCommandMiddleware(new CommandMiddlewareDelegate<TCommand>(commandHandler), command)
                    .Invoke(command)
                    .Wait();
                return;
            default:
                throw new RequestumException($"No handler registered for command type '{typeof(TCommand).Name}'.");
        }
    }

    public Task ExecuteAsync<TCommand>(TCommand command, CancellationToken cancellationToken = default)
        where TCommand : ICommand
    {
        var handler = GetHandler(command);
        switch (handler)
        {
            case IAsyncCommandHandler<TCommand> asyncCommandHandler:
                return BuildCommandMiddleware(new CommandMiddlewareAsyncDelegate<TCommand>(asyncCommandHandler), command)
                    .Invoke(command, cancellationToken);
            case ICommandHandler<TCommand> commandHandler:
                return BuildCommandMiddleware(new CommandMiddlewareDelegate<TCommand>(commandHandler), command)
                    .Invoke(command, cancellationToken);
            default:
                throw new RequestumException($"No handler registered for command type '{typeof(TCommand).Name}'.");
        }
    }

    public TResponse Execute<TCommand, TResponse>(TCommand command)
        where TCommand : ICommand<TResponse>
    {
        var handler = GetHandler(command);
        return handler switch
        {
            ICommandHandler<TCommand, TResponse> commandHandler =>
                BuildCommandMiddleware(new CommandMiddlewareDelegate<TCommand, TResponse>(commandHandler), command)
                    .Invoke(command)
                    .GetAwaiter()
                    .GetResult()!,
            IAsyncCommandHandler<TCommand, TResponse> asyncCommandHandler =>
                BuildCommandMiddleware(new CommandMiddlewareAsyncDelegate<TCommand, TResponse>(asyncCommandHandler), command)
                    .Invoke(command)
                    .GetAwaiter()
                    .GetResult()!,
            _ => throw new RequestumException($"No handler registered for command type '{typeof(TCommand).Name}'."),
        };
    }

    public Task<TResponse> ExecuteAsync<TCommand, TResponse>(TCommand command)
        where TCommand : ICommand<TResponse>
    {
        var handler = GetHandler(command);
        return handler switch
        {
            ICommandHandler<TCommand, TResponse> commandHandler =>
                BuildCommandMiddleware(new CommandMiddlewareDelegate<TCommand, TResponse>(commandHandler), command)
                    .Invoke(command),
            IAsyncCommandHandler<TCommand, TResponse> asyncCommandHandler =>
                BuildCommandMiddleware(new CommandMiddlewareAsyncDelegate<TCommand, TResponse>(asyncCommandHandler), command)
                    .Invoke(command),
            _ => throw new RequestumException($"No handler registered for command type '{typeof(TCommand).Name}'."),
        };
    }

    public void Execute<TCommand>() where TCommand : ICommand, new() =>
        Execute((TCommand)cachedRequests.GetOrAdd(typeof(TCommand), new TCommand()));

    public Task ExecuteAsync<TCommand>(CancellationToken cancellationToken = default) where TCommand : ICommand, new() =>
        ExecuteAsync((TCommand)cachedRequests.GetOrAdd(typeof(TCommand), new TCommand()));

    public void Execute<TCommand, TResponse>() where TCommand : ICommand<TResponse>, new() =>
        Execute<TCommand, TResponse>((TCommand)cachedRequests.GetOrAdd(typeof(TCommand), new TCommand()));

    public Task ExecuteAsync<TCommand, TResponse>(CancellationToken cancellationToken = default) where TCommand : ICommand<TResponse>, new() =>
        ExecuteAsync<TCommand, TResponse>((TCommand)cachedRequests.GetOrAdd(typeof(TCommand), new TCommand()));
}
