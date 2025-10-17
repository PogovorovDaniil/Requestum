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
                BuildMiddleware(new CommandMiddlewareAsyncDelegate<TCommand>(asyncCommandHandler))
                    .Invoke(command)
                    .Wait();
                return;
            case ICommandHandler<TCommand> commandHandler:
                BuildMiddleware(new CommandMiddlewareDelegate<TCommand>(commandHandler))
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
                return BuildMiddleware(new CommandMiddlewareAsyncDelegate<TCommand>(asyncCommandHandler))
                    .Invoke(command, cancellationToken);
            case ICommandHandler<TCommand> commandHandler:
                return BuildMiddleware(new CommandMiddlewareDelegate<TCommand>(commandHandler))
                    .Invoke(command, cancellationToken);
            default:
                throw new RequestumException($"No handler registered for command type '{typeof(TCommand).Name}'.");
        }
    }

    public void Execute<TCommand>() where TCommand : ICommand, new() =>
        Execute((TCommand)cachedRequests.GetOrAdd(typeof(TCommand), new TCommand()));

    public Task ExecuteAsync<TCommand>(CancellationToken cancellationToken = default) where TCommand : ICommand, new() =>
        ExecuteAsync((TCommand)cachedRequests.GetOrAdd(typeof(TCommand), new TCommand()));
}
