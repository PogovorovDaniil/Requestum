using Microsoft.Extensions.DependencyInjection;
using Requestum.Contract;
using Requestum.Policy;

namespace Requestum.Tests;

public record TimeoutTestCommand(int Delay) : ICommand;

[Timeout(1_000)]
public class TimeoutTestHandler : IAsyncCommandHandler<TimeoutTestCommand>
{
    public async Task ExecuteAsync(TimeoutTestCommand command, CancellationToken cancellationToken = default)
    {
        await Task.Delay(command.Delay, cancellationToken);
    }
}

public record RetryTestCommand : ICommand;

[Retry(3)]
public class RetryTestHandler : IAsyncCommandHandler<RetryTestCommand>
{
    public static int RepairAfter = 0;
    public async Task ExecuteAsync(RetryTestCommand command, CancellationToken cancellationToken = default)
    {
        if (RepairAfter > 0)
        {
            RepairAfter--;
            throw new Exception();
        }

        await Task.CompletedTask;
    }
}

public record RetryTimeoutTestQuery : IQuery<RetryTimeoutTestResponse>;
public record RetryTimeoutTestResponse;

[Retry(3)]
[Timeout(TimeoutStep)]
public class RetryTimeoutTestHandler : IAsyncQueryHandler<RetryTimeoutTestQuery, RetryTimeoutTestResponse>
{
    public static int CompletedCount = 0;
    public static int Timeout = 0;
    public const int TimeoutStep = 500;

    public async Task<RetryTimeoutTestResponse> HandleAsync(RetryTimeoutTestQuery query, CancellationToken cancellationToken = default)
    {
        Timeout -= TimeoutStep;
        await Task.Delay(Timeout + TimeoutStep, cancellationToken);

        CompletedCount++;

        return new RetryTimeoutTestResponse();
    }
}

public class PolicyTests
{
    private readonly IRequestum _requestum;

    public PolicyTests()
    {
        var services = new ServiceCollection();
        services.AddRequestum(cfg =>
        {
            cfg.RegisterHandler<TimeoutTestHandler>();
            cfg.RegisterHandler<RetryTestHandler>();
            cfg.RegisterHandler<RetryTimeoutTestHandler>();
        });

        var provider = services.BuildServiceProvider();
        provider.AutoRegisterRequestumPolicies();

        _requestum = provider.GetRequiredService<IRequestum>();
    }

    [Fact]
    public async Task Handle_Command_WithTimeout()
    {
        await _requestum.ExecuteAsync(new TimeoutTestCommand(500));

        await Assert.ThrowsAsync<TimeoutException>(async () =>
        {
            await _requestum.ExecuteAsync(new TimeoutTestCommand(1500));
        });
    }

    [Fact]
    public async Task Handle_Command_WithRetry()
    {
        RetryTestHandler.RepairAfter = 1;
        await _requestum.ExecuteAsync(new RetryTestCommand());

        RetryTestHandler.RepairAfter = 2;
        await _requestum.ExecuteAsync(new RetryTestCommand());

        try
        {
            RetryTestHandler.RepairAfter = 3;
            await _requestum.ExecuteAsync(new RetryTestCommand());
            Assert.Fail();
        }
        catch (AggregateException ex)
        {
            Assert.Equal(3, ex.InnerExceptions.Count);
        }
    }

    [Fact]
    public async Task Handle_Query_WithRetryAndTimeout()
    {
        RetryTimeoutTestHandler.Timeout = RetryTimeoutTestHandler.TimeoutStep * 2 + RetryTimeoutTestHandler.TimeoutStep / 2;
        RetryTimeoutTestHandler.CompletedCount = 0;

        await _requestum.HandleAsync<RetryTimeoutTestQuery, RetryTimeoutTestResponse>(new RetryTimeoutTestQuery());

        Assert.Equal(1, RetryTimeoutTestHandler.CompletedCount);
    }
}
