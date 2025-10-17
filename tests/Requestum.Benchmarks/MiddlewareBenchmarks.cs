using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Requestum.Benchmarks;

/// <summary>
/// Benchmarks comparing MediatR and Requestum middleware/pipeline performance.
/// </summary>
[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.Method)]
public class MiddlewareBenchmarks
{
    private IMediator _mediator = null!;
    private IRequestum _requestum = null!;

    private readonly TestCommandWithMiddleware _commandWithMiddleware = new("Middleware Benchmark");
    private readonly TestAsyncCommandWithMiddleware _asyncCommandWithMiddleware = new("Async Middleware Benchmark");

    [GlobalSetup]
    public void Setup()
    {
        var services = new ServiceCollection();

        // Configure MediatR with pipeline behaviors
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(MediatRHandler).Assembly);
            cfg.AddOpenBehavior(typeof(TestMediatRPipelineBehavior<,>));
        });

        // Configure Requestum with middlewares
        services.AddRequestum(cfg =>
        {
            cfg.RegisterHandlers(typeof(RequestumHandler).Assembly);
            cfg.RegisterMiddleware(typeof(TestAsyncRequestMiddleware<,>));
        });

        var provider = services.BuildServiceProvider();
        _mediator = provider.GetRequiredService<IMediator>();
        _requestum = provider.GetRequiredService<IRequestum>();
    }

    [Benchmark(Baseline = true)]
    public async Task MediatR_CommandWithMiddleware_ExecuteAsync()
    {
        await _mediator.Send(_asyncCommandWithMiddleware);
    }

    [Benchmark]
    public async Task Requestum_CommandWithMiddleware_ExecuteAsync()
    {
        await _requestum.ExecuteAsync(_asyncCommandWithMiddleware);
    }

    [Benchmark]
    public void Requestum_CommandWithMiddleware_ExecuteSync()
    {
        _requestum.Execute(_commandWithMiddleware);
    }
}
