using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Requestum.Benchmarks;

/// <summary>
/// Benchmarks comparing MediatR INotification and Requestum IEventMessage performance.
/// Tests both single and multiple handler scenarios for sync and async event processing.
/// </summary>
[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.Method)]
public class EventMessageBenchmarks
{
    private IMediator _mediatorSingle = null!;
    private IMediator _mediatorMultiple = null!;
    private IRequestum _requestumSingle = null!;
    private IRequestum _requestumMultiple = null!;

    private readonly TestNotification _notification = new("Hello Event!");
    private readonly TestEventMessage _eventMessage = new("Hello Event!");
    private readonly TestAsyncEventMessage _asyncEventMessage = new("Hello Async Event!");

    [GlobalSetup]
    public void Setup()
    {
        // Setup for single handler scenario
        var servicesSingle = new ServiceCollection();
        servicesSingle.AddMediatR(cfg =>
              {
                  cfg.RegisterServicesFromAssemblyContaining<SingleTestNotificationHandler>();
              });
        servicesSingle.AddRequestum(cfg =>
        {
            cfg.RegisterHandler<SingleTestEventMessageReceiver>();
            cfg.RegisterHandler<SingleTestAsyncEventMessageReceiver>();
        });

        var providerSingle = servicesSingle.BuildServiceProvider();
        _mediatorSingle = providerSingle.GetRequiredService<IMediator>();
        _requestumSingle = providerSingle.GetRequiredService<IRequestum>();

        // Setup for multiple handlers scenario (3 handlers per event)
        var servicesMultiple = new ServiceCollection();
        servicesMultiple.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssemblyContaining<FirstTestNotificationHandler>();
        });
        servicesMultiple.AddRequestum(cfg =>
        {
            cfg.RegisterHandler<FirstTestEventMessageReceiver>();
            cfg.RegisterHandler<SecondTestEventMessageReceiver>();
            cfg.RegisterHandler<ThirdTestEventMessageReceiver>();
            cfg.RegisterHandler<FirstTestAsyncEventMessageReceiver>();
            cfg.RegisterHandler<SecondTestAsyncEventMessageReceiver>();
            cfg.RegisterHandler<ThirdTestAsyncEventMessageReceiver>();
        });

        var providerMultiple = servicesMultiple.BuildServiceProvider();
        _mediatorMultiple = providerMultiple.GetRequiredService<IMediator>();
        _requestumMultiple = providerMultiple.GetRequiredService<IRequestum>();
    }

    #region Single Handler Benchmarks
    [Benchmark(Baseline = true)]
    public async Task MediatR_Notification_SingleHandler()
    {
        await _mediatorSingle.Publish(_notification);
    }

    [Benchmark]
    public async Task Requestum_EventMessage_SingleHandler_Async()
    {
        await _requestumSingle.PublishAsync(_asyncEventMessage);
    }

    [Benchmark]
    public void Requestum_EventMessage_SingleHandler_Sync()
    {
        _requestumSingle.Publish(_eventMessage);
    }
    #endregion

    #region Multiple Handlers Benchmarks (3 handlers)
    [Benchmark]
    public async Task MediatR_Notification_MultipleHandlers()
    {
        await _mediatorMultiple.Publish(_notification);
    }

    [Benchmark]
    public async Task Requestum_EventMessage_MultipleHandlers_Async()
    {
        await _requestumMultiple.PublishAsync(_asyncEventMessage);
    }

    [Benchmark]
    public void Requestum_EventMessage_MultipleHandlers_Sync()
    {
        _requestumMultiple.Publish(_eventMessage);
    }
    #endregion
}
