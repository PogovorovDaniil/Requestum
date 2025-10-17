using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Requestum.Benchmarks;

/// <summary>
/// Benchmarks comparing MediatR and Requestum query handling performance.
/// </summary>
[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.Method)]
public class QueryBenchmarks
{
    private IMediator _mediator = null!;
    private IRequestum _requestum = null!;

    private readonly TestQuery _query = new("Query Benchmark");
    private readonly TestAsyncQuery _asyncQuery = new("Async Query Benchmark");

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
    public async Task<TestResponse> MediatR_Query_HandleAsync()
    {
        return await _mediator.Send(_asyncQuery);
    }

    [Benchmark]
    public async Task<TestResponse> Requestum_Query_HandleAsync()
    {
        return await _requestum.HandleAsync<TestAsyncQuery, TestResponse>(_asyncQuery);
    }

    [Benchmark]
    public TestResponse Requestum_Query_HandleSync()
    {
        return _requestum.Handle<TestQuery, TestResponse>(_query);
    }
}
