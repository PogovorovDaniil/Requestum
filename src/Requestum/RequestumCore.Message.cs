using Microsoft.Extensions.DependencyInjection;
using Requestum.Contract;

namespace Requestum;

public partial class RequestumCore
{
    public void Publish<TMessage>(TMessage message)
        where TMessage : IEventMessage
    {
        var handlers = serviceProvider.GetService<IEnumerable<IBaseHandler<TMessage>>>();
        if (handlers is null || !handlers.Any())
        {
            if (RequireEventHandlers)
                throw new RequestumException($"No handlers registered for message type '{typeof(TMessage).Name}'.");
            else return;
        }

        List<Exception> exceptions = [];

        foreach (var handler in handlers)
        {
            try
            {
                switch (handler)
                {
                    case IAsyncEventMessageReceiver<TMessage> asyncMessageReceiver:
                        asyncMessageReceiver.ReceiveAsync(message).Wait();
                        break;
                    case IEventMessageReceiver<TMessage> messageReceiver:
                        messageReceiver.Receive(message);
                        break;
                }
            }
            catch (Exception ex)
            {
                exceptions.Add(ex);
            }
        }

        if (exceptions.Count > 0) 
            throw new AggregateException(exceptions);
    }

    public async Task PublishAsync<TMessage>(TMessage message, CancellationToken cancellationToken = default)
        where TMessage : IEventMessage
    {
        var handlers = serviceProvider.GetService<IEnumerable<IBaseHandler<TMessage>>>(); 
        if (handlers is null || !handlers.Any())
        {
            if (RequireEventHandlers)
                throw new RequestumException($"No handlers registered for message type '{typeof(TMessage).Name}'.");
            else return;
        }

        List<Exception> exceptions = [];

        foreach (var handler in handlers)
        {
            try
            {
                switch (handler)
                {
                    case IAsyncEventMessageReceiver<TMessage> asyncMessageReceiver:
                        await asyncMessageReceiver.ReceiveAsync(message);
                        break;
                    case IEventMessageReceiver<TMessage> messageReceiver:
                        messageReceiver.Receive(message);
                        break;
                }
            }
            catch (Exception exception) 
            {
                exceptions.Add(exception); 
            }
        }

        if (exceptions.Count > 0)
            throw new AggregateException(exceptions);
    }

    public void Publish<TMessage>() where TMessage : IEventMessage, new() => 
        Publish((TMessage)cachedRequests.GetOrAdd(typeof(TMessage), new TMessage()));

    public Task PublishAsync<TMessage>(CancellationToken cancellationToken = default) where TMessage : IEventMessage, new() => 
        PublishAsync((TMessage)cachedRequests.GetOrAdd(typeof(TMessage), new TMessage()));
}
