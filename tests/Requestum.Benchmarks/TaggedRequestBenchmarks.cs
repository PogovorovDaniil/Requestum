using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using Microsoft.Extensions.DependencyInjection;

namespace Requestum.Benchmarks;

/// <summary>
/// Benchmarks comparing tagged vs untagged request execution performance.
/// Tests the overhead of tag-based handler and middleware selection.
/// </summary>
[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.Method)]
public class TaggedRequestBenchmarks
{
    private IRequestum _requestum = null!;

    // Untagged commands
    private readonly TestCommand _untaggedCommand = new("Untagged Command");
    private readonly TestAsyncCommand _untaggedAsyncCommand = new("Untagged Async Command");

    // Tagged commands
    private readonly TaggedBenchmarkCommand _taggedAdminCommand = new("admin", "Tagged Admin Command");
    private readonly TaggedAsyncBenchmarkCommand _taggedAsyncAdminCommand = new("admin", "Tagged Async Admin Command");

    // Untagged queries
    private readonly TestQuery _untaggedQuery = new("Untagged Query");
    private readonly TestAsyncQuery _untaggedAsyncQuery = new("Untagged Async Query");

    // Tagged queries
    private readonly TaggedBenchmarkQuery _taggedAdminQuery = new("admin", "Tagged Admin Query");
    private readonly TaggedAsyncBenchmarkQuery _taggedAsyncAdminQuery = new("admin", "Tagged Async Admin Query");

    [GlobalSetup]
    public void Setup()
    {
        var services = new ServiceCollection();

        services.AddRequestum(cfg =>
        {
            cfg.RegisterHandlers(typeof(TaggedRequestBenchmarks).Assembly);
            
            // Register untagged middleware
            cfg.RegisterMiddleware(typeof(TestAsyncRequestMiddleware<,>));
            
            // Register tagged middleware
            cfg.RegisterMiddleware(typeof(BenchmarkAdminTaggedMiddleware<,>), ServiceLifetime.Singleton);
            cfg.RegisterMiddleware(typeof(BenchmarkUserTaggedMiddleware<,>), ServiceLifetime.Singleton);
        });

        var provider = services.BuildServiceProvider();
        _requestum = provider.GetRequiredService<IRequestum>();
    }

    #region Sync Command Benchmarks

    [Benchmark(Baseline = true)]
    public void Command_Untagged_ExecuteSync()
    {
        _requestum.Execute(_untaggedCommand);
    }

    [Benchmark]
    public void Command_Tagged_ExecuteSync()
    {
        _requestum.Execute(_taggedAdminCommand);
    }

    #endregion

    #region Async Command Benchmarks

    [Benchmark]
    public async Task Command_Untagged_ExecuteAsync()
    {
        await _requestum.ExecuteAsync(_untaggedAsyncCommand);
    }

    [Benchmark]
    public async Task Command_Tagged_ExecuteAsync()
    {
        await _requestum.ExecuteAsync(_taggedAsyncAdminCommand);
    }

    #endregion

    #region Sync Query Benchmarks

    [Benchmark]
    public TestResponse Query_Untagged_HandleSync()
    {
        return _requestum.Handle<TestQuery, TestResponse>(_untaggedQuery);
    }

    [Benchmark]
    public TestResponse Query_Tagged_HandleSync()
    {
        return _requestum.Handle<TaggedBenchmarkQuery, TestResponse>(_taggedAdminQuery);
    }

    #endregion

    #region Async Query Benchmarks

    [Benchmark]
    public async Task<TestResponse> Query_Untagged_HandleAsync()
    {
        return await _requestum.HandleAsync<TestAsyncQuery, TestResponse>(_untaggedAsyncQuery);
    }

    [Benchmark]
    public async Task<TestResponse> Query_Tagged_HandleAsync()
    {
        return await _requestum.HandleAsync<TaggedAsyncBenchmarkQuery, TestResponse>(_taggedAsyncAdminQuery);
    }

    #endregion
}
