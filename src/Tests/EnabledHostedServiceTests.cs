using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Primitives;
using Xunit;

namespace Tests
{
    public class EnabledHostedServiceTests
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
                     services.AddEnabledHostedService<MockHostedService>(sp =>
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
                     services.AddEnabledHostedService<MockHostedService>(sp =>
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
                     services.AddEnabledHostedService<MockHostedService>(sp =>
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
                     services.AddEnabledHostedService<MockHostedService>(sp =>
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
                     services.AddEnabledHostedService<MockHostedService>(sp =>
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

                     services.AddOptionsEnabledHostedService<MockHostedService, TestOptions>(
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