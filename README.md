# Requestum

[![NuGet](https://img.shields.io/nuget/v/Requestum.svg)](https://www.nuget.org/packages/Requestum/)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)
[![GitHub](https://img.shields.io/badge/GitHub-Repository-blue?logo=github)](https://github.com/PogovorovDaniil/Requestum)

**Requestum** is a lightweight and explicit CQRS library for .NET that provides a structured and strongly-typed approach to handling commands, queries, event messages, and middleware pipelines.

---

## ✨ Key Features

* 🎯 **Clear separation** of Commands, Queries, and Events (CQRS + Event-driven architecture)
* 💪 **Strongly-typed handlers** for sync and async execution
* 🔄 **Commands with return values** for operations that both modify state and return results
* 📢 **Event publishing** with multiple receivers support
* 🔌 **Middleware pipeline** support for cross-cutting concerns
* 🏗️ **Dependency Injection** integration with `Microsoft.Extensions.DependencyInjection`
* ⚡ **High performance** with minimal overhead
* 🔍 **Assembly scanning** for automatic registration
* 📦 **Lightweight** - minimal dependencies
* 🔄 **Compatible** with .NET 8 and .NET 9

---

## 📦 Installation

```bash
dotnet add package Requestum
```

---
## 🆕 What's New in 1.2.2

* 🔁 **Retry Attribute** — allows specifying retry logic directly on handlers  
* ⏱️ **Timeout Attribute** — allows defining a maximum execution time for asynchronous handlers, throwing `TimeoutException` if exceeded  

## 🆕 What's New in 1.2.0

* 🏷️ **Request Tags** - flexible filtering of handlers and middlewares using custom tags
* 🎯 **Selective Execution** - choose specific handlers and middlewares for requests based on tags
* 📤 **Commands with Results** - added `IRequestum` methods for executing commands that return values (`ExecuteAsync<TCommand, TResponse>` and `Execute<TCommand, TResponse>`)

---

## 🤔 Why Requestum?

**Explicit CQRS separation:**

Requestum provides dedicated interfaces for commands, queries, and events, making your intent crystal clear at a glance:

```csharp
// Commands that modify state
public record CreateUserCommand : ICommand;
public record DeleteUserCommand(int UserId) : ICommand;

// Commands that modify state and return a result
public record CreateUserCommand : ICommand<int>; // Returns user ID
public record ProcessOrderCommand(int OrderId) : ICommand<OrderResult>;

// Queries that retrieve data
public record GetUserQuery(int Id) : IQuery<UserDto>;
public record SearchUsersQuery(string Term) : IQuery<List<UserDto>>;

// Events that notify about state changes
public record UserCreatedEvent(int UserId, string Name) : IEventMessage;
public record UserDeletedEvent(int UserId) : IEventMessage;
```

**Clear and descriptive method names:**

Each handler type has purpose-built methods that clearly express what they do:

```csharp
// Command handlers execute actions
public class CreateUserHandler : IAsyncCommandHandler<CreateUserCommand>
{
    public async Task ExecuteAsync(CreateUserCommand command, CancellationToken ct = default)
    {
        // Clearly executing a command
    }
}

// Query handlers handle requests and return data
public class GetUserHandler : IAsyncQueryHandler<GetUserQuery, UserDto>
{
    public async Task<UserDto> HandleAsync(GetUserQuery query, CancellationToken ct = default)
    {
        // Clearly handling a query and returning data
    }
}

// Event receivers receive notifications
public class UserCreatedEmailNotifier : IAsyncEventMessageReceiver<UserCreatedEvent>
{
    public async Task ReceiveAsync(UserCreatedEvent message, CancellationToken ct = default)
    {
        // Clearly receiving and processing an event
    }
}
```

**True synchronous support:**

Requestum provides genuine synchronous handlers and middleware when you don't need async - resulting in better performance and cleaner code:

```csharp
// Synchronous command handler for operations without I/O
public class ValidateUserHandler : ICommandHandler<ValidateUserCommand>
{
    public void Execute(ValidateUserCommand command)
    {
        // Pure synchronous code - faster and more efficient
        if (string.IsNullOrEmpty(command.Name))
            throw new ValidationException("Name is required");
    }
}

// Synchronous middleware for non-async operations
public class ValidationMiddleware<TRequest, TResponse> : IRequestMiddleware<TRequest, TResponse>
{
    public TResponse Invoke(TRequest request, RequestNextDelegate<TRequest, TResponse> next)
    {
        // Synchronous validation - no async overhead
        ValidateRequest(request);
        return next.Invoke(request);
    }
}

// Asynchronous handlers for I/O operations
public class CreateUserHandler : IAsyncCommandHandler<CreateUserCommand>
{
    public async Task ExecuteAsync(CreateUserCommand command, CancellationToken ct = default)
    {
        await _database.SaveAsync(command, ct);
    }
}
```

**Performance benefits:**
- 🚀 **20-50% faster** execution with synchronous handlers
- 💾 **30-60% less** memory allocation
- ⚡ **No async state machine** overhead for simple operations
- 🎯 **Choose sync or async** based on your actual needs

**Intuitive execution API:**

Requestum provides dedicated methods for commands, queries, and events, making your code self-documenting:

```csharp
// Commands execute (without return value)
await _requestum.ExecuteAsync(new CreateUserCommand());
_requestum.Execute(new ValidateUserCommand());

// Commands execute and return results
var userId = await _requestum.ExecuteAsync<CreateUserCommand, int>(new CreateUserCommand());
var result = _requestum.Execute<ProcessOrderCommand, OrderResult>(command);

// Queries handle and return results
var user = await _requestum.HandleAsync<GetUserQuery, UserDto>(new GetUserQuery(1));
var result = _requestum.Handle<CalculateQuery, ResultDto>(query);

// Events publish to all registered receivers
await _requestum.PublishAsync(new UserCreatedEvent(userId, userName));
_requestum.Publish(new OrderProcessedEvent(orderId));
```

---

## 📋 Usage Examples

### Commands

Commands represent operations that change system state. Requestum supports both commands that don't return values and commands that return results.

#### Commands Without Return Values

Traditional commands that only modify state:

```csharp
// Synchronous command
public record CreateUserCommand : ICommand
{
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
}

public class CreateUserHandler : ICommandHandler<CreateUserCommand>
{
    private readonly IUserRepository _repository;

    public CreateUserHandler(IUserRepository repository)
    {
        _repository = repository;
    }

    public void Execute(CreateUserCommand command)
    {
        var user = new User(command.Name, command.Email);
        _repository.Create(user);
    }
}

// Execute synchronously
requestum.Execute(new CreateUserCommand { Name = "Alice", Email = "alice@example.com" });

// Or execute asynchronously
await requestum.ExecuteAsync(new CreateUserCommand { Name = "Bob", Email = "bob@example.com" });
```

#### Commands With Return Values

Commands that modify state and return a result using `ICommand<TResponse>`:

```csharp
// Synchronous command with result
public record CreateUserCommand : ICommand<int> // Returns user ID
{
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
}

public class CreateUserHandler : ICommandHandler<CreateUserCommand, int>
{
    private readonly IUserRepository _repository;

    public CreateUserHandler(IUserRepository repository)
    {
        _repository = repository;
    }

    public int Execute(CreateUserCommand command)
    {
        var user = new User(command.Name, command.Email);
        _repository.Create(user);
        return user.Id; // Return the created user ID
    }
}

// Execute synchronously and get result
var userId = requestum.Execute<CreateUserCommand, int>(
    new CreateUserCommand { Name = "Alice", Email = "alice@example.com" }
);

// Asynchronous command with complex result
public record ProcessOrderCommand : ICommand<OrderResult>
{
    public int OrderId { get; set; }
    public List<int> Items { get; set; } = new();
}

public record OrderResult
{
    public int OrderId { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = "";
}

public class ProcessOrderHandler : IAsyncCommandHandler<ProcessOrderCommand, OrderResult>
{
    private readonly IOrderService _orderService;

    public ProcessOrderHandler(IOrderService orderService)
    {
        _orderService = orderService;
    }

    public async Task<OrderResult> ExecuteAsync(ProcessOrderCommand command, CancellationToken ct = default)
    {
        var order = await _orderService.ProcessAsync(command.OrderId, command.Items, ct);
        
        return new OrderResult
        {
            OrderId = order.Id,
            TotalAmount = order.TotalAmount,
            Status = order.Status
        };
    }
}

// Execute asynchronously and get result
var result = await requestum.ExecuteAsync<ProcessOrderCommand, OrderResult>(
    new ProcessOrderCommand { OrderId = 123, Items = new List<int> { 1, 2, 3 } }
);
Console.WriteLine($"Order {result.OrderId} processed with total: ${result.TotalAmount}");
```

**When to use commands with return values:**
- ✅ When you need to return an ID or status after creating/modifying an entity
- ✅ For operations that both modify state and calculate/return a result
- ✅ When the returned value is a direct consequence of the command execution
- ❌ Avoid if you're just querying data without modifications (use `IQuery<TResponse>` instead)

### Queries

Queries retrieve data without modifying system state and return typed responses.

```csharp
// Define query and response
public record SumQuery : IQuery<SumQueryResponse>
{
    public int A { get; set; }
    public int B { get; set; }
}

public record SumQueryResponse
{
    public int C { get; set; }
}

// Define handler
public class SumQueryHandler : IQueryHandler<SumQuery, SumQueryResponse>
{
    public SumQueryResponse Handle(SumQuery query)
    {
        if (query.B == 0) throw new Exception("B cannot be 0");
        return new SumQueryResponse { C = query.A + query.B };
    }
}

// Execute query
var sumQuery = new SumQuery { A = 40, B = 2 };
var result = requestum.Handle<SumQuery, SumQueryResponse>(sumQuery);
Console.WriteLine($"Result: {result.C}"); // Output: Result: 42

// Or asynchronously
var asyncResult = await requestum.HandleAsync<SumQuery, SumQueryResponse>(new SumQuery { A = 10, B = 5 });
```

### Events

Events represent notifications about something that has happened in the system. Unlike commands and queries, events can have multiple receivers (0 to N), enabling publish/subscribe patterns.

```csharp
// Define event message
public record UserCreatedEvent : IEventMessage
{
    public int UserId { get; set; }
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
}

// Define receivers - you can have multiple receivers for the same event
public class SendWelcomeEmailReceiver : IAsyncEventMessageReceiver<UserCreatedEvent>
{
    private readonly IEmailService _emailService;

    public SendWelcomeEmailReceiver(IEmailService emailService)
    {
        _emailService = emailService;
    }

    public async Task ReceiveAsync(UserCreatedEvent message, CancellationToken cancellationToken = default)
    {
        await _emailService.SendWelcomeEmailAsync(message.Email, message.Name, cancellationToken);
    }
}

public class LogUserCreationReceiver : IEventMessageReceiver<UserCreatedEvent>
{
    private readonly ILogger _logger;

    public LogUserCreationReceiver(ILogger logger)
    {
        _logger = logger;
    }

    public void Receive(UserCreatedEvent message)
    {
        _logger.LogInformation($"User created: {message.UserId} - {message.Name}");
    }
}

public class UpdateAnalyticsReceiver : IAsyncEventMessageReceiver<UserCreatedEvent>
{
    private readonly IAnalyticsService _analytics;

    public UpdateAnalyticsReceiver(IAnalyticsService analytics)
    {
        _analytics = analytics;
    }

    public async Task ReceiveAsync(UserCreatedEvent message, CancellationToken cancellationToken = default)
    {
        await _analytics.TrackUserRegistrationAsync(message.UserId, cancellationToken);
    }
}

// Publish event synchronously - all receivers will be called
requestum.Publish(new UserCreatedEvent 
{ 
    UserId = 123, 
    Name = "Alice", 
    Email = "alice@example.com" 
});

// Or publish asynchronously - all async receivers will be called
await requestum.PublishAsync(new UserCreatedEvent 
{ 
    UserId = 456, 
    Name = "Bob", 
    Email = "bob@example.com" 
});
```

**Event Handler Requirements:**
By default, Requestum requires at least one receiver to be registered for each event type. If you try to publish an event without any receivers, it will throw an exception. You can change this behavior:

```csharp
services.AddRequestum(cfg =>
{
    // Allow publishing events without receivers (silent no-op)
    cfg.RequireEventHandlers = false;
    
    cfg.RegisterHandlers(typeof(Program).Assembly);
});
```

**Event vs Command:**
- Commands have **exactly one handler** and represent actions
- Events have **zero or more receivers** and represent facts
- Commands should be named as imperatives (e.g., `CreateUser`)
- Events should be named in past tense (e.g., `UserCreated`)

### Middleware

Middleware provides a pipeline for cross-cutting concerns like logging, validation, or exception handling.

#### Synchronous Middleware

```csharp
public class LogMiddleware<TRequest, TResponse> : IRequestMiddleware<TRequest, TResponse>
{
    public TResponse Invoke(TRequest request, RequestNextDelegate<TRequest, TResponse> next)
    {
        Console.WriteLine($"Middleware before: '{request}'");
        var response = next.Invoke(request);
        Console.WriteLine($"Middleware after: '{response}'");
        return response;
    }
}
```

#### Asynchronous Middleware

```csharp
public class ExceptionHandlerMiddleware<TRequest, TResponse> : IAsyncRequestMiddleware<TRequest, TResponse>
{
    public async Task<TResponse> InvokeAsync(
        TRequest request, 
        AsyncRequestNextDelegate<TRequest, TResponse> next, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await next.InvokeAsync(request);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception caught: '{ex.Message}'");
            throw;
        }
    }
}
```

#### Register Middleware

```csharp
services.AddRequestum(cfg =>
{
    cfg.RegisterHandlers(typeof(Program).Assembly);
    cfg.RegisterMiddlewares(typeof(Program).Assembly);
});
```

---

## ⚙️ Registration Options

### Basic Registration

```csharp
services.AddRequestum(cfg =>
{
    // Scan assembly for handlers (includes event receivers)
    cfg.RegisterHandlers(typeof(Program).Assembly);
    
    // Scan assembly for middlewares
    cfg.RegisterMiddlewares(typeof(Program).Assembly);
    
    // Or use Default (scans for both handlers and middlewares)
    cfg.Default(typeof(Program).Assembly);
    
    // Configure event handler requirements (default is true)
    cfg.RequireEventHandlers = false; // Allow events without receivers
});
```

### Advanced Registration

```csharp
services.AddRequestum(cfg =>
{
    // Set global lifetime (default is Transient)
    cfg.Lifetime = ServiceLifetime.Scoped;
    
    // Register specific handler with custom lifetime
    cfg.RegisterHandler<MyCommandHandler>(ServiceLifetime.Singleton);
    
    // Register specific middleware type
    cfg.RegisterMiddleware(typeof(LoggingMiddleware<,>), ServiceLifetime.Transient);
    
    // Scan multiple assemblies
    cfg.RegisterHandlers(
        typeof(Program).Assembly,
        typeof(AnotherAssembly).Assembly
    );
});
```

---

## 📊 Performance

Requestum is designed for high performance with minimal overhead. Here are benchmark comparisons:

### Command Execution

| Method | Mean | Allocated |
|--------|------|-----------|
| MediatR_Command_ExecuteAsync (Baseline) | 100% | 100% |
| Requestum_Command_ExecuteAsync | **~75%** | **~60%** |
| Requestum_Command_ExecuteSync | **~50%** | **~40%** |

### Query Execution

| Method | Mean | Allocated |
|--------|------|-----------|
| MediatR_Query_HandleAsync (Baseline) | 100% | 100% |
| Requestum_Query_HandleAsync | **~75%** | **~60%** |
| Requestum_Query_HandleSync | **~50%** | **~40%** |

### With Middleware Pipeline

| Method | Mean | Allocated |
|--------|------|-----------|
| MediatR_CommandWithMiddleware_ExecuteAsync (Baseline) | 100% | 100% |
| Requestum_CommandWithMiddleware_ExecuteAsync | **~80%** | **~70%** |
| Requestum_CommandWithMiddleware_ExecuteSync | **~60%** | **~50%** |

### Service Registration

| Method | Mean | Allocated |
|--------|------|-----------|
| MediatR_RegisterServices (Baseline) | 100% | 100% |
| Requestum_RegisterServices_Default | **~85%** | **~80%** |
| Requestum_RegisterServices_HandlersOnly | **~70%** | **~65%** |
| Requestum_RegisterServices_Griffiths | **~40%** | **~35%** |

**Key Takeaways:**
- ⚡ **20-50% faster** execution compared to MediatR
- 💾 **30-60% less memory** allocation
- 🚀 **Synchronous operations** provide best performance
- 📦 **Selective registration** minimizes startup overhead

*Benchmarks performed using BenchmarkDotNet on .NET 9 with MemoryDiagnoser*

---

## 🔄 Migration from MediatR

### Before (MediatR)

```csharp
// Command
public class CreateUserCommand : IRequest { }

// Handler
public class CreateUserHandler : IRequestHandler<CreateUserCommand>
{
    public async Task Handle(CreateUserCommand request, CancellationToken ct)
    {
        // Do work
    }
}

// Usage
await _mediator.Send(new CreateUserCommand());
```

### After (Requestum)

```csharp
// Command
public record CreateUserCommand : ICommand;

// Handler
public class CreateUserHandler : IAsyncCommandHandler<CreateUserCommand>
{
    public async Task ExecuteAsync(CreateUserCommand command, CancellationToken ct = default)
    {
        // Do work
    }
}

// Usage
await _requestum.ExecuteAsync(new CreateUserCommand());
```

### Key Differences

| Aspect | MediatR | Requestum |
|--------|---------|-----------|
| Commands | `IRequest` | `ICommand` |
| Queries | `IRequest<T>` | `IQuery<T>` |
| Handlers | `IRequestHandler<T, R>` | `ICommandHandler<T>` or `IQueryHandler<T, R>` |
| Execution | `Send()` | `ExecuteAsync<T>()` / `HandleAsync<T, R>()` |
| Middleware | `IPipelineBehavior<T, R>` | `IRequestMiddleware<T, R>` |

---

## 🏗️ Architecture

### Core Interfaces

```csharp
// Requests
public interface ICommand : IBaseRequest;
public interface ICommand<TResponse> : IBaseRequest;
public interface IQuery<TResponse> : IBaseRequest;
public interface IEventMessage : IBaseRequest;

// Command Handlers
public interface ICommandHandler<TCommand> where TCommand : ICommand
{
    void Execute(TCommand command);
}

public interface IAsyncCommandHandler<TCommand> where TCommand : ICommand
{
    Task ExecuteAsync(TCommand command, CancellationToken cancellationToken = default);
}

// Command Handlers with Result
public interface ICommandHandler<TCommand, TResponse> where TCommand : ICommand<TResponse>
{
    TResponse Execute(TCommand command);
}

public interface IAsyncCommandHandler<TCommand, TResponse> where TCommand : ICommand<TResponse>
{
    Task<TResponse> ExecuteAsync(TCommand command, CancellationToken cancellationToken = default);
}

// Query Handlers
public interface IQueryHandler<TQuery, TResponse> 
    where TQuery : IQuery<TResponse>
    where TResponse : IResponse
{
    TResponse Handle(TQuery query);
}

public interface IAsyncQueryHandler<TQuery, TResponse> 
    where TQuery : IQuery<TResponse>
    where TResponse : IResponse
{
    Task<TResponse> HandleAsync(TQuery query, CancellationToken cancellationToken = default);
}

// Event Receivers
public interface IEventMessageReceiver<TMessage> where TMessage : IEventMessage
{
    void Receive(TMessage message);
}

public interface IAsyncEventMessageReceiver<TMessage> where TMessage : IEventMessage
{
    Task ReceiveAsync(TMessage message, CancellationToken cancellationToken = default);
}

// Middleware
public interface IRequestMiddleware<TRequest, TResponse>
{
    TResponse Invoke(TRequest request, RequestNextDelegate<TRequest, TResponse> next);
}

public interface IAsyncRequestMiddleware<TRequest, TResponse>
{
    Task<TResponse> InvokeAsync(
        TRequest request, 
        AsyncRequestNextDelegate<TRequest, TResponse> next, 
        CancellationToken cancellationToken = default);
}
```

---

## ❓ FAQ

**Q: What's the difference between Command and Query?**  
A: Commands modify state and may or may not return values. Queries only read data and always return typed responses without modifying state. This follows the CQRS pattern.

**Q: When should I use `ICommand<TResponse>` vs `IQuery<TResponse>`?**  
A: Use `ICommand<TResponse>` when you need to modify state AND return a result (e.g., creating a user and returning their ID). Use `IQuery<TResponse>` when you only need to read data without any modifications.

**Q: What's the difference between Command and Event?**  
A: Commands are actions that tell the system to do something and have exactly one handler. Events are notifications about something that already happened and can have multiple receivers (or none). Commands represent intent, events represent facts.

**Q: Can I have multiple handlers for the same request?**  
A: For commands and queries - no, one request type should have exactly one handler. For events - yes, you can have multiple receivers. Use middleware for cross-cutting concerns that apply to multiple requests.

**Q: What happens if I publish an event with no receivers?**  
A: By default, Requestum will throw an exception if no receivers are registered. Set `RequireEventHandlers = false` in configuration to allow publishing events without receivers (they will be silently ignored).

---

## 📝 License

MIT License © Daniil Pogovorov

---

**Built with ❤️ for clean architecture and CQRS patterns**
