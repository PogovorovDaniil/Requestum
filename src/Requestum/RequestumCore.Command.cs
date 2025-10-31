using Microsoft.Extensions.DependencyInjection;
using Requestum.Contract;
using Requestum.Middleware;

namespace Requestum;

public partial class RequestumCore
{
    public void Execute<TCommand>(TCommand command)
        where TCommand : ICommand
    {
        var handler = serviceProvider.GetService(typeof(IBaseHandler<TCommand>));
        switch (handler)
        {
            case IAsyncCommandHandler<TCommand> asyncCommandHandler:
                BuildMiddleware(new CommandMiddlewareAsyncDelegate<TCommand>(asyncCommandHandler), RequestType.Command)
                    .Invoke(command)
                    .Wait();
                return;
            case ICommandHandler<TCommand> commandHandler:
                BuildMiddleware(new CommandMiddlewareDelegate<TCommand>(commandHandler), RequestType.Command)
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
        var handler = serviceProvider.GetService(typeof(IBaseHandler<TCommand>));
        switch (handler)
        {
            case IAsyncCommandHandler<TCommand> asyncCommandHandler:
                return BuildMiddleware(new CommandMiddlewareAsyncDelegate<TCommand>(asyncCommandHandler), RequestType.Command)
                    .Invoke(command, cancellationToken);
            case ICommandHandler<TCommand> commandHandler:
                return BuildMiddleware(new CommandMiddlewareDelegate<TCommand>(commandHandler), RequestType.Command)
                    .Invoke(command, cancellationToken);
            default:
                throw new RequestumException($"No handler registered for command type '{typeof(TCommand).Name}'.");
        }
    }

    public TResponse Execute<TCommand, TResponse>(TCommand command)
        where TCommand : ICommand<TResponse>
    {
        var handler = serviceProvider.GetService<IBaseHandler<TCommand>>();
        return handler switch
        {
            ICommandHandler<TCommand, TResponse> commandHandler =>
                BuildMiddleware(new CommandMiddlewareDelegate<TCommand, TResponse>(commandHandler), RequestType.Command)
                    .Invoke(command)
                    .GetAwaiter()
                    .GetResult()!,
            IAsyncCommandHandler<TCommand, TResponse> asyncCommandHandler =>
                BuildMiddleware(new CommandMiddlewareAsyncDelegate<TCommand, TResponse>(asyncCommandHandler), RequestType.Command)
                    .Invoke(command)
                    .GetAwaiter()
                    .GetResult()!,
            _ => throw new RequestumException($"No handler registered for command type '{typeof(TCommand).Name}'."),
        };
    }

    public Task<TResponse> ExecuteAsync<TCommand, TResponse>(TCommand command)
        where TCommand : ICommand<TResponse>
    {
        var handler = serviceProvider.GetService<IBaseHandler<TCommand>>();
        return handler switch
        {
            ICommandHandler<TCommand, TResponse> commandHandler =>
                BuildMiddleware(new CommandMiddlewareDelegate<TCommand, TResponse>(commandHandler), RequestType.Command)
                    .Invoke(command),
            IAsyncCommandHandler<TCommand, TResponse> asyncCommandHandler =>
                BuildMiddleware(new CommandMiddlewareAsyncDelegate<TCommand, TResponse>(asyncCommandHandler), RequestType.Command)
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
