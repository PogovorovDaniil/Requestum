namespace Requestum.Contract;

/// <summary>
/// Base interface for all request types supported by Requestum.
/// </summary>
public interface IBaseRequest;

/// <summary>
/// Represents a command that modifies the system state.
/// </summary>
public interface ICommand : IBaseRequest;

/// <summary>
/// Represents a query returning a result of the specified type.
/// </summary>
/// <typeparam name="TResponse">The type of the response returned by the query.</typeparam>
public interface IQuery<TResponse> : IBaseRequest;

/// <summary>
/// Represents an event message that can be published and handled by event handlers.
/// </summary>
public interface IEventMessage : IBaseRequest;