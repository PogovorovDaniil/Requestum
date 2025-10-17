using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using Microsoft.Extensions.DependencyInjection;

namespace Requestum.Benchmarks;

/// <summary>
/// Benchmarks comparing MediatR and Requestum service registration performance.
/// </summary>
[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.Method)]
public class RegistrationBenchmarks
{
    [Benchmark(Baseline = true)]
    public IServiceProvider MediatR_RegisterServices()
    {
        var services = new ServiceCollection();

        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(MediatRHandler).Assembly);
            cfg.AddOpenBehavior(typeof(TestMediatRPipelineBehavior<,>));
        });

        return services.BuildServiceProvider();
    }

    [Benchmark]
    public IServiceProvider Requestum_RegisterServices_Default()
    {
        var services = new ServiceCollection();

        services.AddRequestum(cfg =>
        {
            cfg.Default(typeof(RequestumHandler).Assembly);
        });

        return services.BuildServiceProvider();
    }

    [Benchmark]
    public IServiceProvider Requestum_RegisterServices_Separate()
    {
        var services = new ServiceCollection();

        services.AddRequestum(cfg =>
        {
            cfg.RegisterHandlers(typeof(RequestumHandler).Assembly);
            cfg.RegisterMiddlewares(typeof(RequestumHandler).Assembly);
        });

        return services.BuildServiceProvider();
    }

    [Benchmark]
    public IServiceProvider Requestum_RegisterServices_HandlersOnly()
    {
        var services = new ServiceCollection();

        services.AddRequestum(cfg =>
        {
            cfg.RegisterHandlers(typeof(RequestumHandler).Assembly);
        });

        return services.BuildServiceProvider();
    }

    [Benchmark]
    public IServiceProvider Requestum_RegisterServices_Specific()
    {
        var services = new ServiceCollection();

        services.AddRequestum(cfg =>
        {
            cfg.RegisterHandler<RequestumHandler>();
            cfg.RegisterMiddleware(typeof(TestRequestMiddleware<,>));
        });

        return services.BuildServiceProvider();
    }
}
