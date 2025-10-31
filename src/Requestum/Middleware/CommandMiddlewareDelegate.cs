using Requestum.Contract;
using System.Runtime.CompilerServices;

namespace Requestum.Middleware;

internal readonly struct CommandMiddlewareDelegate<TCommand>(ICommandHandler<TCommand> commandHandler) : IMiddlewareDelegate<TCommand, EmptyResponse>
    where TCommand : ICommand
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task<EmptyResponse> Invoke(TCommand request, CancellationToken cancellationToken = default)
    {
        commandHandler.Execute(request);
        return new ValueTask<EmptyResponse>(EmptyResponse.Instance).AsTask();
    }
}

internal readonly struct CommandMiddlewareAsyncDelegate<TCommand>(IAsyncCommandHandler<TCommand> commandHandler) : IMiddlewareDelegate<TCommand, EmptyResponse>
    where TCommand : ICommand
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public async Task<EmptyResponse> Invoke(TCommand request, CancellationToken cancellationToken = default)
    {
        await commandHandler.ExecuteAsync(request, cancellationToken);
        return EmptyResponse.Instance;
    }
}

internal readonly struct CommandMiddlewareDelegate<TCommand, TResponse>(ICommandHandler<TCommand, TResponse> commandHandler) : IMiddlewareDelegate<TCommand, TResponse>
    where TCommand : ICommand<TResponse>
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task<TResponse> Invoke(TCommand request, CancellationToken cancellationToken = default)
    {
        return new ValueTask<TResponse>(commandHandler.Execute(request)).AsTask();
    }
}

internal readonly struct CommandMiddlewareAsyncDelegate<TCommand, TResponse>(IAsyncCommandHandler<TCommand, TResponse> commandHandler) : IMiddlewareDelegate<TCommand, TResponse>
    where TCommand : ICommand<TResponse>
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task<TResponse> Invoke(TCommand request, CancellationToken cancellationToken = default)
    {
        return commandHandler.ExecuteAsync(request);
    }
}
