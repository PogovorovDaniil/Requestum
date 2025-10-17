using Requestum.Contract;
using System.Runtime.CompilerServices;

namespace Requestum.Middleware;

internal readonly struct CommandMiddlewareDelegate<TCommand>(ICommandHandler<TCommand> commandHandler) : IMiddlewareDelegate<TCommand, CommandResponse>
    where TCommand : ICommand
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task<CommandResponse> Invoke(TCommand request, CancellationToken cancellationToken = default)
    {
        commandHandler.Execute(request);
        return new ValueTask<CommandResponse>(CommandResponse.Instance).AsTask();
    }
}

internal readonly struct CommandMiddlewareAsyncDelegate<TCommand>(IAsyncCommandHandler<TCommand> commandHandler) : IMiddlewareDelegate<TCommand, CommandResponse>
    where TCommand : ICommand
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public async Task<CommandResponse> Invoke(TCommand request, CancellationToken cancellationToken = default)
    {
        await commandHandler.ExecuteAsync(request, cancellationToken);
        return CommandResponse.Instance;
    }
}
