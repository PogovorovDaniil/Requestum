using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Requestum.Benchmarks;

/// <summary>
/// Benchmarks comparing MediatR and Requestum command execution performance.
/// </summary>
[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.Method)]
public class CommandBenchmarks
{
    private IMediator _mediator = null!;
    private IRequestum _requestum = null!;

    private readonly TestCommand _command = new("Hello Benchmark!");
    private readonly TestAsyncCommand _asyncCommand = new("Hello Async Benchmark!");

    [GlobalSetup]
    public void Setup()
    {
        var services = new ServiceCollection();

        // Configure MediatR
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(MediatRHandler).Assembly);
        });

        // Configure Requestum
        services.AddRequestum(cfg =>
        {
            cfg.RegisterHandlers(typeof(RequestumHandler).Assembly);
        });

        var provider = services.BuildServiceProvider();
        _mediator = provider.GetRequiredService<IMediator>();
        _requestum = provider.GetRequiredService<IRequestum>();
    }

    [Benchmark(Baseline = true)]
    public async Task MediatR_Command_ExecuteAsync()
    {
        await _mediator.Send(_asyncCommand);
    }

    [Benchmark]
    public async Task Requestum_Command_ExecuteAsync()
    {
        await _requestum.ExecuteAsync(_asyncCommand);
    }

    [Benchmark]
    public void Requestum_Command_ExecuteSync()
    {
        _requestum.Execute(_command);
    }
}
