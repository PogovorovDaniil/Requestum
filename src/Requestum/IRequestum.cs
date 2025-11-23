using Requestum.Contract;

namespace Requestum;

/// <summary>
/// Central interface of the Requestum library, responsible for registering and executing
/// command, query, and middleware handlers.
/// </summary>
public interface IRequestum
{
    #region Command

    /// <summary>
    /// Executes a command synchronously.
    /// </summary>
    /// <typeparam name="TCommand">The command type implementing <see cref="ICommand"/>.</typeparam>
    /// <param name="command">The command to execute.</param>
    void Execute<TCommand>(TCommand command)
        where TCommand : ICommand;

    /// <summary>
    /// Executes a command asynchronously.
    /// </summary>
    /// <typeparam name="TCommand">The command type implementing <see cref="ICommand"/>.</typeparam>
    /// <param name="command">The command to execute.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task ExecuteAsync<TCommand>(TCommand command, CancellationToken cancellationToken = default)
        where TCommand : ICommand;

    /// <summary>
    /// Executes a command synchronously by creating a new instance of the command type.
    /// </summary>
    /// <typeparam name="TCommand">The command type implementing <see cref="ICommand"/> with a parameterless constructor.</typeparam>
    void Execute<TCommand>()
        where TCommand : ICommand, new();

    /// <summary>
    /// Executes a command asynchronously by creating a new instance of the command type.
    /// </summary>
    /// <typeparam name="TCommand">The command type implementing <see cref="ICommand"/> with a parameterless constructor.</typeparam>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task ExecuteAsync<TCommand>(CancellationToken cancellationToken = default)
        where TCommand : ICommand, new();

    /// <summary>
    /// Executes a command with result synchronously and returns the response.
    /// </summary>
    /// <typeparam name="TCommand">The command type implementing <see cref="ICommand{TResponse}"/>.</typeparam>
    /// <typeparam name="TResponse">The response type.</typeparam>
    /// <param name="command">The command to execute.</param>
    /// <returns>The command response.</returns>
    TResponse Execute<TCommand, TResponse>(TCommand command)
        where TCommand : ICommand<TResponse>;

    /// <summary>
    /// Executes a command with result asynchronously and returns the response.
    /// </summary>
    /// <typeparam name="TCommand">The command type implementing <see cref="ICommand{TResponse}"/>.</typeparam>
    /// <typeparam name="TResponse">The response type.</typeparam>
    /// <param name="command">The command to execute.</param>
    /// <returns>The command response.</returns>
    Task<TResponse> ExecuteAsync<TCommand, TResponse>(TCommand command)
        where TCommand : ICommand<TResponse>;

    /// <summary>
    /// Executes a command with result synchronously by creating a new instance of the command type and returns the response.
    /// </summary>
    /// <typeparam name="TCommand">The command type implementing <see cref="ICommand{TResponse}"/> with a parameterless constructor.</typeparam>
    /// <typeparam name="TResponse">The response type.</typeparam>
    /// <returns>The command response.</returns>
    void Execute<TCommand, TResponse>()
        where TCommand : ICommand<TResponse>, new();

    /// <summary>
    /// Executes a command with result asynchronously by creating a new instance of the command type and returns the response.
    /// </summary>
    /// <typeparam name="TCommand">The command type implementing <see cref="ICommand{TResponse}"/> with a parameterless constructor.</typeparam>
    /// <typeparam name="TResponse">The response type.</typeparam>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The command response.</returns>
    Task ExecuteAsync<TCommand, TResponse>(CancellationToken cancellationToken = default)
        where TCommand : ICommand<TResponse>, new();

    #endregion

    #region Query

    /// <summary>
    /// Handles a query synchronously and returns the response.
    /// </summary>
    /// <typeparam name="TQuery">The query type implementing <see cref="IQuery{TResponse}"/>.</typeparam>
    /// <typeparam name="TResponse">The response type.</typeparam>
    /// <param name="query">The query to handle.</param>
    /// <returns>The query response.</returns>
    TResponse Handle<TQuery, TResponse>(TQuery query)
        where TQuery : IQuery<TResponse>;

    /// <summary>
    /// Handles a query asynchronously and returns the response.
    /// </summary>
    /// <typeparam name="TQuery">The query type implementing <see cref="IQuery{TResponse}"/>.</typeparam>
    /// <typeparam name="TResponse">The response type.</typeparam>
    /// <param name="query">The query to handle.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The query response.</returns>
    Task<TResponse> HandleAsync<TQuery, TResponse>(TQuery query, CancellationToken cancellationToken = default)
        where TQuery : IQuery<TResponse>;
    
    /// <summary>
    /// Handles a query synchronously by creating a new instance of the query type and returns the response.
    /// </summary>
    /// <typeparam name="TQuery">The query type implementing <see cref="IQuery{TResponse}"/> with a parameterless constructor.</typeparam>
    /// <typeparam name="TResponse">The response type.</typeparam>
    /// <returns>The query response.</returns>
    TResponse Handle<TQuery, TResponse>()
        where TQuery : IQuery<TResponse>, new();
    
    /// <summary>
    /// Handles a query asynchronously by creating a new instance of the query type and returns the response.
    /// </summary>
    /// <typeparam name="TQuery">The query type implementing <see cref="IQuery{TResponse}"/> with a parameterless constructor.</typeparam>
    /// <typeparam name="TResponse">The response type.</typeparam>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The query response.</returns>
    Task<TResponse> HandleAsync<TQuery, TResponse>(CancellationToken cancellationToken = default)
        where TQuery : IQuery<TResponse>, new();

    #endregion

    #region Message

    /// <summary>
    /// Gets a value indicating whether event handlers are required to be registered for event messages.
    /// When <c>true</c>, attempting to publish an event message without any registered receivers will throw an exception.
    /// When <c>false</c>, publishing an event message without receivers will be silently ignored.
    /// Default value is <c>true</c>.
    /// </summary>
    /// <value>
    /// <c>true</c> if at least one event handler must be registered; otherwise, <c>false</c>.
    /// </value>
    bool RequireEventHandlers { get; }

    /// <summary>
    /// Publishes an event message synchronously to all registered receivers.
    /// </summary>
    /// <typeparam name="TMessage">The event message type implementing <see cref="IEventMessage"/>.</typeparam>
    /// <param name="message">The event message to publish.</param>
    void Publish<TMessage>(TMessage message)
        where TMessage : IEventMessage;

    /// <summary>
    /// Publishes an event message asynchronously to all registered receivers.
    /// </summary>
    /// <typeparam name="TMessage">The event message type implementing <see cref="IEventMessage"/>.</typeparam>
    /// <param name="message">The event message to publish.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task PublishAsync<TMessage>(TMessage message, CancellationToken cancellationToken = default)
        where TMessage : IEventMessage;

    /// <summary>
    /// Publishes an event message synchronously to all registered receivers by creating a new instance of the message type.
    /// </summary>
    /// <typeparam name="TMessage">The event message type implementing <see cref="IEventMessage"/> with a parameterless constructor.</typeparam>
    void Publish<TMessage>()
        where TMessage : IEventMessage, new();
    
    /// <summary>
    /// Publishes an event message asynchronously to all registered receivers by creating a new instance of the message type.
    /// </summary>
    /// <typeparam name="TMessage">The event message type implementing <see cref="IEventMessage"/> with a parameterless constructor.</typeparam>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task PublishAsync<TMessage>(CancellationToken cancellationToken = default)
        where TMessage : IEventMessage, new();

    #endregion

    #region Policy

    /// <summary>
    /// Adds a retry policy for the specified asynchronous handler,
    /// defining how many times it should be re-attempted if it fails.
    /// </summary>
    /// <param name="type">The handler type for which retries are configured.</param>
    /// <param name="retryCount">The number of retry attempts.</param>
    void AddHandlerRetry(Type type, int retryCount);

    /// <summary>
    /// Sets an execution timeout for the specified asynchronous handler.
    /// If the handler runs longer than the provided duration,
    /// a <see cref="TimeoutException"/> will be thrown.
    /// </summary>
    /// <param name="type">The handler type for which the timeout is configured.</param>
    /// <param name="timeout">The maximum allowed execution time.</param>
    void AddHandlerTimeout(Type type, TimeSpan timeout);

    #endregion
}
