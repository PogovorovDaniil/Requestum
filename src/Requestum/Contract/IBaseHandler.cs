namespace Requestum.Contract;

/// <summary>
/// Base interface for all command and query handlers.
/// </summary>
public interface IBaseHandler;

/// <summary>
/// Represents a generic request handler for a specific request type.
/// </summary>
/// <typeparam name="TRequest">The type of request this handler can process.</typeparam>
public interface IBaseHandler<TRequest> : IBaseHandler 
    where TRequest : IBaseRequest;

/// <summary>
/// Defines a handler for a command.
/// </summary>
/// <typeparam name="TCommand">The type of command this handler processes.</typeparam>
public interface ICommandHandler<TCommand> : IBaseHandler<TCommand>
    where TCommand : ICommand
{
    /// <summary>
    /// Executes the specified command.
    /// </summary>
    /// <param name="command">The command to execute.</param>
    void Execute(TCommand command);
}

/// <summary>
/// Defines an asynchronous handler for a command.
/// </summary>
/// <typeparam name="TCommand">The type of command this handler processes.</typeparam>
public interface IAsyncCommandHandler<TCommand> : IBaseHandler<TCommand>
    where TCommand : ICommand
{
    /// <summary>
    /// Asynchronously executes the specified command.
    /// </summary>
    /// <param name="command">The command to execute.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task ExecuteAsync(TCommand command, CancellationToken cancellationToken = default);
}

/// <summary>
/// Defines a handler for a query that returns a result.
/// </summary>
/// <typeparam name="TQuery">The type of query.</typeparam>
/// <typeparam name="TResponse">The type of the query result.</typeparam>
public interface IQueryHandler<TQuery, TResponse> : IBaseHandler<TQuery>
    where TQuery : IQuery<TResponse>
{
    /// <summary>
    /// Handles the query and returns the result.
    /// </summary>
    /// <param name="query">The query to handle.</param>
    /// <returns>The result of the query.</returns>
    TResponse Handle(TQuery query);
}

/// <summary>
/// Defines an asynchronous handler for a query that returns a result.
/// </summary>
/// <typeparam name="TQuery">The type of query.</typeparam>
/// <typeparam name="TResponse">The type of the query result.</typeparam>
public interface IAsyncQueryHandler<TQuery, TResponse> : IBaseHandler<TQuery>
    where TQuery : IQuery<TResponse>
{
    /// <summary>
    /// Asynchronously handles the query and returns the result.
    /// </summary>
    /// <param name="query">The query to handle.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The result of the query.</returns>
    Task<TResponse> HandleAsync(TQuery query, CancellationToken cancellationToken = default);
}

/// <summary>
/// Defines a handler for an message that can be received synchronously.
/// </summary>
/// <typeparam name="TMessage">The type of the message.</typeparam>
public interface IEventMessageReceiver<TMessage> : IBaseHandler<TMessage>
    where TMessage : IEventMessage
{
    /// <summary>
    /// Receives the event message to the handler.
    /// </summary>
    /// <param name="message">The event message to receive.</param>
    void Receive(TMessage message);
}

/// <summary>
/// Defines a handler for an message that can be received asynchronously.
/// </summary>
/// <typeparam name="TMessage">The type of the message.</typeparam>
public interface IAsyncEventMessageReceiver<TMessage> : IBaseHandler<TMessage>
    where TMessage : IEventMessage
{
    /// <summary>
    /// Asynchronously receives the message to the handler.
    /// </summary>
    /// <param name="message">The message to receive.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task ReceiveAsync(TMessage message, CancellationToken cancellationToken = default);
}