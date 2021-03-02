using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;

namespace Dazinator.ResponsiveCore.ResponsiveHostedService
{
    public class ResponsiveHostedServiceOptionsBuilder<THostedService> : IResponsiveHostedServiceOptionsBuilder<THostedService>
    {
        public ResponsiveHostedServiceOptionsBuilder(IServiceProvider services)
        {
            Services = services;
            ServiceResolver = ActivatorUtilities.GetServiceOrCreateInstance<THostedService>;
            //  ServiceOptions = new ResponsiveHostedServiceOptions(Services);
            // ChangeTokenFactoryResolver = (sp) => () => EmptyChangeToken.Instance;
        }

        public Func<IServiceProvider, THostedService> ServiceResolver { get; set; }
        //  public ResponsiveHostedServiceOptions ServiceOptions { get; set; }
        public IServiceProvider Services { get; }

        /// <summary>
        /// Use a factory Func to create the instance of the IHosted service that will be controlled by the enabled / disabled check. If you don't set this, by default <see cref="ActivatorUtilities.GetServiceOrCreateInstance<typeparamref name="THostedService"/>"/> is used.
        /// </summary>
        /// <param name="resolver"></param>
        /// <returns></returns>
        public IResponsiveHostedServiceOptionsBuilder SetServiceFactory(Func<IServiceProvider, THostedService> resolver)
        {
            ServiceResolver = resolver;
            return this;
        }

        /// <summary>
        /// Use a factory Func to create the instance of the IHosted service that will be controlled by the enabled / disabled check. If you don't set this, by default <see cref="ActivatorUtilities.GetServiceOrCreateInstance<typeparamref name="THostedService"/>"/> is used.
        /// </summary>
        /// <param name="resolver"></param>
        /// <returns></returns>
        public IResponsiveHostedServiceOptionsBuilder SetServiceFactory(Func<THostedService> resolver)
        {
            ServiceResolver = (s) => resolver();
            return this;
        }

        public Func<IChangeToken> ChangeTokenProducer { get; set; } = () => EmptyChangeToken.Instance;
        public IDisposable ChangeTokenProducerLifetime { get; set; } = null;
        public Func<CancellationToken, Task<bool>> ShouldBeRunningAsyncCheck { get; set; } = (c) => Task.FromResult(true);
        public IResponsiveHostedServiceOptionsBuilder RespondsTo(Func<IChangeToken> resolver, IDisposable lifetime, int debounceDelayInMs = ResponsiveHostedServiceOptions.DefaultDebounceDelayInMs)
        {
            ChangeTokenProducer = resolver;
            ChangeTokenProducerLifetime = lifetime;
            DebounceDelayInMilliseconds = debounceDelayInMs;
            return this;
        }


        public int DebounceDelayInMilliseconds { get; set; } = ResponsiveHostedServiceOptions.DefaultDebounceDelayInMs;

        public void ShouldBeRunning(Func<bool> shouldBeRunningCheck)
        {
            ShouldBeRunningAsyncCheck = (cancelToken) =>
            {
                var result = shouldBeRunningCheck();
                return Task.FromResult(result);
            };
        }

        public void WithAsyncShouldBeRunningCheck(Func<CancellationToken, Task<bool>> shouldBeRunningAsyncCheck)
        {
            ShouldBeRunningAsyncCheck = shouldBeRunningAsyncCheck;
        }

        public ResponsiveHostedServiceOptions Build()
        {
            return new ResponsiveHostedServiceOptions(ChangeTokenProducer, ChangeTokenProducerLifetime, ShouldBeRunningAsyncCheck);
        }

    }


}