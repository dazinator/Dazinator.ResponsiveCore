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
    public class UnitTest1
    {
        [Fact]
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
            //.ConfigureWebHostDefaults(webBuilder =>
            //{
            //    webBuilder.UseStartup<Startup>();
            //    webBuilder.UseWebRoot("wwwroot");
            //});
        }

        [Fact]
        public async Task DisabledService_IsNotStartedOrStopped_WhenHostStartsAndStops()
        {

            bool startCalled = false;
            bool stopCalled = false;

            CancellationTokenSource latestCancellationTokenSource = null;

            var changeTokenFactory = FuncUtils
                                .KeepLatest<CancellationTokenSource>(onNewInstance: a => { latestCancellationTokenSource = a; })
                                .Convert(a => new CancellationChangeToken(a.Token));
            //.Select(a => new CancellationChangeToken(a));

            bool isServiceEnabled = false;

            string[] args = null;
            var host = Host.CreateDefaultBuilder(args)
                 .ConfigureServices(services =>
                 {
                     AddReloadableHostedService<MockHostedService>(services, sp =>
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

                     }, changeTokenFactory, (s) =>
                     {
                         return isServiceEnabled;
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

            CancellationTokenSource latestCancellationTokenSource = null;

            var changeTokenFactory = FuncUtils
                                .KeepLatest<CancellationTokenSource>(onNewInstance: a => { latestCancellationTokenSource = a; })
                                .Convert(a => new CancellationChangeToken(a.Token));
            //.Select(a => new CancellationChangeToken(a));

            bool isServiceEnabled = false;

            string[] args = null;
            var host = Host.CreateDefaultBuilder(args)
                 .ConfigureServices(services =>
                 {
                     AddReloadableHostedService<MockHostedService>(services, sp =>
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

                     }, changeTokenFactory, () =>
                     {
                         return isServiceEnabled;
                     });

                 });

            var result = await host.StartAsync();
            Assert.False(startCalled);
            Assert.False(stopCalled);

            // trigger change token to apply the enabled state.
            isServiceEnabled = true;
            latestCancellationTokenSource.Cancel();

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

            CancellationTokenSource latestCancellationTokenSource = null;

            var changeTokenFactory = FuncUtils
                                .KeepLatest<CancellationTokenSource>(onNewInstance: a =>
                                {
                                    latestCancellationTokenSource = a;
                                })
                                .Convert(a => new CancellationChangeToken(a.Token));
            //.Select(a => new CancellationChangeToken(a));

            bool isServiceEnabled = false;

            string[] args = null;
            var host = Host.CreateDefaultBuilder(args)
                 .ConfigureServices(services =>
                 {
                     AddReloadableHostedService<MockHostedService>(services, sp =>
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

                     }, changeTokenFactory, () =>
                     {
                         return isServiceEnabled;
                     });

                 });

            var result = await host.StartAsync();
            Assert.False(startCalled);
            Assert.False(stopCalled);

            // trigger change token to apply the enabled state.
            isServiceEnabled = true;
            latestCancellationTokenSource.Cancel();
            // wait small delay to give time for async reload to complete.
            await Task.Delay(1000);

            Assert.True(startCalled);
            Assert.False(stopCalled);

            startCalled = false;
            // enable again
            isServiceEnabled = true;
            latestCancellationTokenSource.Cancel();
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

            CancellationTokenSource latestCancellationTokenSource = null;

            var changeTokenFactory = FuncUtils
                                .KeepLatest<CancellationTokenSource>(onNewInstance: a => { latestCancellationTokenSource = a; })
                                .Convert(a => new CancellationChangeToken(a.Token));
            //.Select(a => new CancellationChangeToken(a));

            bool isServiceEnabled = true;

            string[] args = null;
            var host = Host.CreateDefaultBuilder(args)
                 .ConfigureServices(services =>
                 {
                     AddReloadableHostedService<MockHostedService>(services, sp =>
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

                     }, changeTokenFactory, () =>
                     {
                         return isServiceEnabled;
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

            CancellationTokenSource latestCancellationTokenSource = null;

            var changeTokenFactory = FuncUtils
                                .KeepLatest<CancellationTokenSource>(onNewInstance: a => { latestCancellationTokenSource = a; })
                                .Convert(a => new CancellationChangeToken(a.Token));
            //.Select(a => new CancellationChangeToken(a));

            bool isServiceEnabled = true;

            string[] args = null;
            var host = Host.CreateDefaultBuilder(args)
                 .ConfigureServices(services =>
                 {
                     AddReloadableHostedService<MockHostedService>(services, sp =>
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

                     }, changeTokenFactory, () =>
                     {
                         return isServiceEnabled;
                     });

                 });

            var result = await host.StartAsync();
            Assert.True(startCalled);
            Assert.False(stopCalled);

            startCalled = false;

            // trigger change token to reload the hosted service.
            isServiceEnabled = false;
            latestCancellationTokenSource.Cancel();

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

                     AddEnabledDisabledHostedService<MockHostedService, TestOptions>(services,
                         sp => mockedService,
                         options =>
                         {
                             return options.IsEnabled;
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

        #region Options Pattern
        public IServiceCollection AddEnabledDisabledHostedService<TService, TOptions>(
           IServiceCollection services,
           Predicate<TOptions> shouldBeRunning, string optionsName = default)
           where TService : IHostedService
        {

            return AddEnabledDisabledHostedService<TService>(services,
                (sp) =>
                {
                    var monitor = sp.GetRequiredService<IOptionsMonitor<TOptions>>();
                    var changeTokenFactory = ChangeTokenFactoryHelper.UseCallbackRegistrations((onChangedCallback) =>
                    {
                        return monitor.OnChange(a => onChangedCallback());
                    });
                    return changeTokenFactory;
                },
                (sp) =>
                {
                    var monitor = sp.GetRequiredService<IOptionsMonitor<TOptions>>();
                    if (string.IsNullOrWhiteSpace(optionsName))
                    {
                        return shouldBeRunning(monitor.CurrentValue);
                    }
                    else
                    {
                        return shouldBeRunning(monitor.Get(optionsName));
                    }
                });
        }

        public IServiceCollection AddEnabledDisabledHostedService<TService, TOptions>(
          IServiceCollection services,
          Func<IServiceProvider, TService> innerServiceFactory,
          Predicate<TOptions> shouldBeRunning, string optionsName = default)
          where TService : IHostedService
        {

            return AddReloadableHostedService<TService>(services,
                innerServiceFactory,
                (sp) =>
                {
                    var monitor = sp.GetRequiredService<IOptionsMonitor<TOptions>>();
                    var changeTokenFactory = ChangeTokenFactoryHelper.UseCallbackRegistrations((onChangedCallback) =>
                    {
                        return monitor.OnChange(a => onChangedCallback());
                    });
                    return changeTokenFactory;
                },
                (sp) =>
                {
                    var monitor = sp.GetRequiredService<IOptionsMonitor<TOptions>>();
                    if (string.IsNullOrWhiteSpace(optionsName))
                    {
                        return shouldBeRunning(monitor.CurrentValue);
                    }
                    else
                    {
                        return shouldBeRunning(monitor.Get(optionsName));
                    }
                });
        }
        #endregion Options Pattern


        /// <summary>
        /// Register an<see cref= "IHostedService" /> that can be started and stopped at runtime by signalling an <see cref = "IChangeToken" /> to apply the current enabled / disabled state. This state is obtained via a callback that will be invoked when the change token is signalled.
        /// </summary>
        /// <typeparam name = "TService" ></ typeparam >
        /// < param name= "services" ></ param >
        /// < param name= "changeTokenFactory" > Factory that produces <see cref = "IChangeToken" /></ param > s, used to signal the service to re-check it's latest enabled / disabled state via the <param name="shouldBeRunning"/> callback.
        /// <param name = "shouldBeRunning" > A delegate that returns whether the service is currently enabled or not. 
        /// If the service is enabled but not currently running it will be started.
        /// If it is not enabled but is currently running it will be stopped.
        /// </param>
        /// <returns></returns>
        public IServiceCollection AddEnabledDisabledHostedService<TService>(IServiceCollection services,
            Func<IServiceProvider, Func<IChangeToken>> getChangeTokenFactory,
            Func<IServiceProvider, bool> shouldBeRunning)
            where TService : IHostedService
        {
            return AddReloadableHostedService<TService>(services,
                (sp) => ActivatorUtilities.CreateInstance<TService>(sp),
                getChangeTokenFactory,
                shouldBeRunning);
        }

        public IServiceCollection AddReloadableHostedService<TService>(
            IServiceCollection services,
            Func<IServiceProvider, TService> innerServiceFactory,
            Func<IServiceProvider, Func<IChangeToken>> getChangeTokenFactory,
            Func<IServiceProvider, bool> shouldBeRunning)
            where TService : IHostedService
        {
            services.AddHostedService<ReloadableHostedService<TService>>((sp) =>
            {
                var inner = innerServiceFactory(sp);
                var tokenFactory = getChangeTokenFactory.Invoke(sp);
                return new ReloadableHostedService<TService>(inner,
                    tokenFactory,
                    () => shouldBeRunning(sp));
            });
            return services;
        }

        public IServiceCollection AddReloadableHostedService<TService>(
           IServiceCollection services,
           Func<IServiceProvider, TService> innerServiceFactory,
           Func<IChangeToken> getChangeTokenFactory,
           Func<IServiceProvider, bool> shouldBeRunning)
           where TService : IHostedService
        {
            return AddReloadableHostedService<TService>(services, innerServiceFactory, (sp) => getChangeTokenFactory, shouldBeRunning);
        }

        public IServiceCollection AddReloadableHostedService<TService>(
          IServiceCollection services,
          Func<IServiceProvider, TService> innerServiceFactory,
          Func<IChangeToken> getChangeTokenFactory,
          Func<bool> shouldBeRunning)
          where TService : IHostedService
        {
            return AddReloadableHostedService<TService>(services, innerServiceFactory, (sp) => getChangeTokenFactory, (sp) => shouldBeRunning?.Invoke() ?? true);
        }
    }



    public class ReloadableHostedService<TInner> : IHostedService, IDisposable
        where TInner : IHostedService
    {
        private readonly TInner _inner;
        private readonly Func<IChangeToken> _changeTokenFactory;
        private readonly Func<bool> _shouldBeRunning;
        private IDisposable _listening;

        private bool _isRunning;
        private bool _disposedValue;

        // this service decorates an inner service,
        // but allows it to be started or stopped at runtime
        // based on a change token that gets triggered, to re-evaluate whether
        // it should be currently running or not. Based on that evaluation and the current state of the service,
        // StartAsync or StopAsync will be called.
        public ReloadableHostedService(TInner inner,
            Func<IChangeToken> changeTokenFactory,
            Func<bool> shouldBeRunning)
        {
            _inner = inner;
            _changeTokenFactory = changeTokenFactory;
            _shouldBeRunning = shouldBeRunning;
            _listening = ChangeTokenHelper.OnChangeDebounce(_changeTokenFactory, InvokeChanged, delayInMilliseconds: 500);
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await EnsureInnerAsync(cancellationToken);
        }

        private void InvokeChanged()
        {
            EnsureInnerAsync().Forget();
        }

        public async Task EnsureInnerAsync(CancellationToken hostCancellationToken = default)
        {
            // check what the desired state for this service is
            // by invoking a predicate - should it be running or not?
            // then enact that desired state by comparing against the IsRunning and starting or stopping accordingy.

            bool shouldBeRunning = _shouldBeRunning?.Invoke() ?? true;
            // TODO: may need to lock in case Start or Stop is currently running and hasn't finished toggling the _isRunning flag yet.
            if (_isRunning && !shouldBeRunning)
            {

                var token = default(CancellationToken); // todo: should probably use a proper token here with a time specified.
                await StopInnerAsync(token);
            }
            else if (!_isRunning && shouldBeRunning)
            {
                var token = default(CancellationToken); // todo: should probably use a proper token here with a time specified.
                await StartInnerAsync(token);
            }
        }

        public async Task StopInnerAsync(CancellationToken cancellationToken)
        {
            if (_isRunning)
            {
                await _inner.StopAsync(cancellationToken);
                _isRunning = false;
            }
        }

        public async Task StartInnerAsync(CancellationToken cancellationToken)
        {
            if (!_isRunning)
            {
                await _inner.StartAsync(cancellationToken);
                _isRunning = true;
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            var token = cancellationToken;
            await StopInnerAsync(token);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                    _listening?.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
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

}
