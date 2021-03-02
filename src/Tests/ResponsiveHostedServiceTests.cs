using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Xunit;

namespace Tests
{
    public class ResponsiveHostedServiceTests
    {
        [Fact(Skip = "Testing assumption for how existing functionality works in the box")]
        public async Task Can_Test_Hosted_Service_Starts_And_Stops()
        {

            bool startCalled = false;
            bool stopCalled = false;

            string[] args = null;
            var host = Host.CreateDefaultBuilder(args)
                 .ConfigureServices(services =>
                 {
                     services.AddHostedService<MockHostedService>(sp => new MockHostedService(
                         onStartAsync: async (cancelToken) =>
                        {
                            startCalled = true;
                        },
                        onStopAsync: async (cancelToken) =>
                        {
                            stopCalled = true;
                        }));
                 });

            var result = await host.StartAsync();
            Assert.True(startCalled);

            await result.StopAsync();
            Assert.True(stopCalled);
        }

        [Fact]
        public async Task DisabledService_IsNotStartedOrStopped_WhenHostStartsAndStops()
        {

            bool startCalled = false;
            bool stopCalled = false;

            bool isServiceEnabled = false;
            Action trigger;

            string[] args = null;
            var host = Host.CreateDefaultBuilder(args)
                 .ConfigureServices(services =>
                 {
                     services.AddResponsiveHostedService<MockHostedService>(a =>
                     {
                         a.SetServiceFactory(sp =>
                         {
                             var mockedService = new MockHostedService(
                             onStartAsync: async (cancelToken) =>
                             {
                                 startCalled = true;
                             },
                            onStopAsync: async (cancelToken) =>
                            {
                                stopCalled = true;
                            });
                             return mockedService;

                         })
                         .ShouldBeRunning(() =>
                             {
                                 return isServiceEnabled;
                             });
                     });
                 });

            var result = await host.StartAsync();
            Assert.False(startCalled);

            await result.StopAsync();
            Assert.False(stopCalled);
        }

        [Fact]
        public async Task DisabledService_IsStarted_AfterBeingEnabled()
        {

            bool startCalled = false;
            bool stopCalled = false;

            bool isServiceEnabled = false;
            Action trigger = null;

            string[] args = null;
            var host = Host.CreateDefaultBuilder(args)
                 .ConfigureServices(services =>
                 {
                     services.AddResponsiveHostedService<MockHostedService>(a =>
                     {
                         var tokenProducer = new ChangeTokenProducerBuilder()
                                                .IncludeTrigger(out trigger)
                                                 .Build(out var disposable);

                         a.SetServiceFactory(sp =>
                         {
                             var mockedService = new MockHostedService(
                             onStartAsync: async (cancelToken) =>
                             {
                                 startCalled = true;
                             },
                            onStopAsync: async (cancelToken) =>
                            {
                                stopCalled = true;
                            });
                             return mockedService;

                         })
                         .RespondsTo(tokenProducer, disposable)
                                        .ShouldBeRunning(() =>
                                        {
                                            return isServiceEnabled;
                                        });

                     });
                 });

            var result = await host.StartAsync();
            Assert.False(startCalled);
            Assert.False(stopCalled);

            // trigger change token to apply the enabled state.
            isServiceEnabled = true;
            trigger();

            // wait small delay to give time for async reload to complete.
            await Task.Delay(1000);

            Assert.True(startCalled);
            Assert.False(stopCalled);

            startCalled = false;
            await result.StopAsync();

            Assert.False(startCalled);
            Assert.True(stopCalled);
        }

        [Fact]
        public async Task DisabledService_IsStartedOnlyOnce_AfterBeingEnabledMultipleTimesInARow()
        {

            bool startCalled = false;
            bool stopCalled = false;

            Action trigger = null;
            bool isServiceEnabled = false;

            string[] args = null;
            var host = Host.CreateDefaultBuilder(args)
                 .ConfigureServices(services =>
                 {
                     var tokenProducer = new ChangeTokenProducerBuilder()
                                               .IncludeTrigger(out trigger)
                                                .Build(out var disposable);
                     services.AddResponsiveHostedService<MockHostedService>(a =>
                     {
                         a.SetServiceFactory(sp =>
                         {
                             var mockedService = new MockHostedService(
                             onStartAsync: async (cancelToken) =>
                             {
                                 startCalled = true;
                             },
                            onStopAsync: async (cancelToken) =>
                            {
                                stopCalled = true;
                            });
                             return mockedService;
                         })
                         .RespondsTo(tokenProducer, disposable)
                            .ShouldBeRunning(() =>
                            {
                                return isServiceEnabled;
                            });
                     });

                 });

            var result = await host.StartAsync();
            Assert.False(startCalled);
            Assert.False(stopCalled);

            // trigger change token to apply the enabled state.
            isServiceEnabled = true;
            trigger();
            // wait small delay to give time for async reload to complete.
            await Task.Delay(1000);

            Assert.True(startCalled);
            Assert.False(stopCalled);

            startCalled = false;
            // enable again
            isServiceEnabled = true;
            trigger();
            // wait small delay to give time for async reload to complete.
            await Task.Delay(500);
            // shouldn't be started a second time as service already running.
            Assert.False(startCalled);
            Assert.False(stopCalled);

            startCalled = false;
            await result.StopAsync();

            Assert.False(startCalled);
            Assert.True(stopCalled);
        }

        [Fact]
        public async Task EnabledService_IsStarted_WhenHostStarts()
        {

            bool startCalled = false;
            bool stopCalled = false;

            bool isServiceEnabled = true;

            string[] args = null;
            var host = Host.CreateDefaultBuilder(args)
                 .ConfigureServices(services =>
                 {
                     services.AddResponsiveHostedService<MockHostedService>(a =>
                     {
                         a.SetServiceFactory(sp =>
                         {
                             var mockedService = new MockHostedService(
                             onStartAsync: async (cancelToken) =>
                             {
                                 startCalled = true;
                             },
                            onStopAsync: async (cancelToken) =>
                            {
                                stopCalled = true;
                            });
                             return mockedService;

                         })
                         .ShouldBeRunning(() =>
                         {
                             return isServiceEnabled;
                         });
                     });

                 });

            var result = await host.StartAsync();
            Assert.True(startCalled);
            Assert.False(stopCalled);

            startCalled = false;
            await result.StopAsync();

            Assert.False(startCalled);
            Assert.True(stopCalled);
        }

        [Fact]
        public async Task EnabledService_IsStopped_AfterBeingDisabled()
        {

            bool startCalled = false;
            bool stopCalled = false;

            bool isServiceEnabled = true;
            Action trigger = null;

            string[] args = null;
            var host = Host.CreateDefaultBuilder(args)
                 .ConfigureServices(services =>
                 {
                     services.AddResponsiveHostedService<MockHostedService>(a =>
                     {
                         var tokenProducer = new ChangeTokenProducerBuilder()
                                             .IncludeTrigger(out trigger)
                                              .Build(out var disposable);

                         a.SetServiceFactory(sp =>
                         {
                             var mockedService = new MockHostedService(
                             onStartAsync: async (cancelToken) =>
                             {
                                 startCalled = true;
                             },
                            onStopAsync: async (cancelToken) =>
                            {
                                stopCalled = true;
                            });
                             return mockedService;

                         })
                         .RespondsTo(tokenProducer, disposable)
                         .ShouldBeRunning(() =>
                         {
                             return isServiceEnabled;
                         });
                     });

                 });

            var result = await host.StartAsync();
            Assert.True(startCalled);
            Assert.False(stopCalled);

            startCalled = false;

            // trigger change token to reload the hosted service.
            isServiceEnabled = false;
            trigger();

            await result.StopAsync();

            Assert.False(startCalled);
            Assert.True(stopCalled);
        }

        [Fact]
        public async Task EnabledService_CanBeRegistered_UsingOptionsPattern()
        {

            bool startCalled = false;
            bool stopCalled = false;

            //CancellationTokenSource latestCancellationTokenSource = null;

            //var changeTokenFactory = FuncUtils
            //                    .KeepLatest<CancellationTokenSource>(onNewInstance: a => { latestCancellationTokenSource = a; })
            //                    .Convert(a => new CancellationChangeToken(a.Token));
            //.Select(a => new CancellationChangeToken(a));

            bool isServiceEnabled = true;

            var mockedService = new MockHostedService(
                         onStartAsync: async (cancelToken) =>
                         {
                             startCalled = true;
                         },
                        onStopAsync: async (cancelToken) =>
                        {
                            stopCalled = true;
                        });


            string[] args = null;
            var host = Host.CreateDefaultBuilder(args)
                 .ConfigureServices(services =>
                 {
                     services.Configure<TestOptions>((a) =>
                     {
                         a.IsEnabled = isServiceEnabled;
                     });

                     services.AddResponsiveHostedService<MockHostedService>(
                         a =>
                         {
                             var monitor = a.Services.GetRequiredService<IOptionsMonitor<TestOptions>>();

                             var tokenProducer = new ChangeTokenProducerBuilder()
                                             .IncludeSubscribingHandlerTrigger((trigger) => monitor.OnChange((o, n) => trigger()))
                                             .Build(out var disposable);

                             a.SetServiceFactory(sp => mockedService)
                              .RespondsTo(tokenProducer, disposable)
                             .ShouldBeRunning(() =>
                             {
                                 return monitor.CurrentValue?.IsEnabled ?? false;
                             });
                         });
                 });

            var result = await host.StartAsync();
            Assert.True(startCalled);
            Assert.False(stopCalled);

            startCalled = false;
            await result.StopAsync();

            Assert.False(startCalled);
            Assert.True(stopCalled);
        }

        public class TestOptions
        {
            public bool IsEnabled { get; set; }

        }


        [Fact]
        public async Task EnabledService_CanBeRegistered_UsingEventHandlerListener()
        {

            bool startCalled = false;
            bool stopCalled = false;
            bool isServiceEnabled = true;

            var mockedService = new MockHostedService(
                         onStartAsync: async (cancelToken) =>
                         {
                             startCalled = true;
                         },
                        onStopAsync: async (cancelToken) =>
                        {
                            stopCalled = true;
                        });

            var classWithEvent = new TestClassWithAnEvent();
            IDisposable eventSubscription = null;

            string[] args = null;
            var host = Host.CreateDefaultBuilder(args)
                 .ConfigureServices(services =>
                 {
                     services.AddSingleton<TestClassWithAnEvent>(classWithEvent);

                     services.AddResponsiveHostedService<MockHostedService>(
                         a =>
                         {
                             var tokenProducer = new ChangeTokenProducerBuilder()
                                            .IncludeEventHandlerTrigger<string>(
                                                addHandler: (handler) => classWithEvent.SomeEvent += handler,
                                                removeHandler: (handler) => classWithEvent.SomeEvent -= handler,
                                                (disposable) => eventSubscription = disposable)
                                            .Build(out var disposable);

                             a.SetServiceFactory(sp => mockedService)
                             .RespondsTo(tokenProducer, disposable)
                             .ShouldBeRunning(() => isServiceEnabled);
                         });
                 });

            var result = await host.StartAsync();
            Assert.True(startCalled);
            Assert.False(stopCalled);

            // Now we want the service to be stopped
            isServiceEnabled = false;
            startCalled = false;
            classWithEvent.RaiseEvent(true);
            // give some time for the service to respond and stop itslef.
            await Task.Delay(2000);
            Assert.False(startCalled);
            Assert.True(stopCalled);
        }


        [Fact]
        public async Task EnabledService_CanBeRegistered_UsingRequirementsPattern()
        {

            bool startCalled = false;
            bool stopCalled = false;

            //CancellationTokenSource latestCancellationTokenSource = null;

            //var changeTokenFactory = FuncUtils
            //                    .KeepLatest<CancellationTokenSource>(onNewInstance: a => { latestCancellationTokenSource = a; })
            //                    .Convert(a => new CancellationChangeToken(a.Token));
            //.Select(a => new CancellationChangeToken(a));

            bool isServiceEnabled = true;

            var mockedService = new MockHostedService(
                         onStartAsync: async (cancelToken) =>
                         {
                             startCalled = true;
                         },
                        onStopAsync: async (cancelToken) =>
                        {
                            stopCalled = true;
                        });


            string[] args = null;
            var host = Host.CreateDefaultBuilder(args)
                 .ConfigureServices(services =>
                 {

                     services.AddResponsiveHostedService<MockHostedService>(
                         a =>
                         {
                             var monitor = a.Services.GetRequiredService<IOptionsMonitor<TestOptions>>();

                             var tokenProducer = new ChangeTokenProducerBuilder()
                                             .IncludeSubscribingHandlerTrigger((trigger) => monitor.OnChange((o, n) => trigger()))
                                             .Build(out var disposable);

                             a.SetServiceFactory(sp => mockedService)
                              .RespondsTo(tokenProducer, disposable)
                              .Requires((b) =>
                              {
                                  // if any return false, the service will not start (or be stopped if its running).
                                  b.IncludeFunc(token => isServiceEnabled)
                                   .IncludeFunc(token => isServiceEnabled)
                                   .IncludeFunc(token => isServiceEnabled);
                              });
                         });
                 });

            var result = await host.StartAsync();
            Assert.True(startCalled);
            Assert.False(stopCalled);

            startCalled = false;
            await result.StopAsync();

            Assert.False(startCalled);
            Assert.True(stopCalled);
        }

    }

    public class TestClassWithAnEvent
    {
        public bool IsServiceEnabled
        {
            get;
            private set;
        } = false;

        public event EventHandler<string> SomeEvent;

        public void RaiseEvent(bool isServiceEnabled)
        {
            IsServiceEnabled = isServiceEnabled;
            SomeEvent?.Invoke(this, "Fired");
        }
    }

    public class MockHostedService : IHostedService
    {
        private readonly Func<CancellationToken, Task> _onStartAsync;
        private readonly Func<CancellationToken, Task> _onStopAsync;

        public MockHostedService(Func<CancellationToken, Task> onStartAsync, Func<CancellationToken, Task> onStopAsync)
        {
            _onStartAsync = onStartAsync;
            _onStopAsync = onStopAsync;
        }
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            if (_onStartAsync != null)
            {
                await _onStartAsync(cancellationToken);
            }
        }
        public async Task StopAsync(CancellationToken cancellationToken)
        {
            if (_onStopAsync != null)
            {
                await _onStopAsync(cancellationToken);
            }
        }
    }


    //public class PollyCircuitBreakerRequirement : IRequirement
    //{
    //    private readonly ILogger<PollyCircuitBreakerRequirement> _logger;
    //    private readonly ICircuitBreakerPolicy _policy;
    //    private readonly CircuitState[] _allowedStates;
    //    private readonly Predicate<ICircuitBreakerPolicy> _check;

    //    public PollyCircuitBreakerRequirement(
    //        ILogger<PollyCircuitBreakerRequirement> logger,
    //        ICircuitBreakerPolicy policy,
    //        params CircuitState[] allowedStates)
    //    {
    //        _logger = logger;
    //        _policy = policy;
    //        _allowedStates = allowedStates;
    //    }

    //    public Task<bool> IsSatisfied(CancellationToken cancellationToken)
    //    {
    //        if (_policy == null)
    //        {
    //            _logger.LogWarning("Policy null");
    //            return Task.FromResult(false);
    //        }
    //        var state = _policy.CircuitState;
    //        _logger.LogDebug("Circuit status: {status}", state);
    //        if (_allowedStates == null || _allowedStates.Length == 0)
    //        {
    //            _logger.LogWarning("No allowed states.");
    //            return Task.FromResult(false);
    //        }
    //        var allowed = _allowedStates.Contains(state);
    //        return Task.FromResult(allowed);
    //    }
    //}

}