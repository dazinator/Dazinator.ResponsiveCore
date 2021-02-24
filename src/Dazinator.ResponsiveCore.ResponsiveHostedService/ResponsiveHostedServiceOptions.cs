using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;

namespace Dazinator.ResponsiveCore.ResponsiveHostedService
{

    public class ResponsiveHostedServiceOptions
    {
        public ResponsiveHostedServiceOptions(IServiceProvider services)
        {
            Services = services;
        }

        public IServiceProvider Services { get; set; }
        public Func<IChangeToken> ChangeTokenProducer { get; set; } = () => EmptyChangeToken.Instance;
        public IDisposable ChangeTokenProducerLifetime { get; set; } = null;
        public Func<CancellationToken, Task<bool>> ShouldBeRunningAsyncCheck { get; set; } = (c) => Task.FromResult(true);
        public ResponsiveHostedServiceOptions RespondsTo(Func<IChangeToken> resolver, IDisposable lifetime)
        {
            ChangeTokenProducer = resolver;
            ChangeTokenProducerLifetime = lifetime;
            return this;
        }

        public ResponsiveHostedServiceOptions WithShouldBeRunningCheck(Func<bool> shouldBeRunningCheck)
        {
            ShouldBeRunningAsyncCheck = (cancelToken) =>
                {
                    var result = shouldBeRunningCheck();
                    return Task.FromResult(result);
                };
            return this;
        }

        public ResponsiveHostedServiceOptions WithAsyncShouldBeRunningCheck(Func<CancellationToken, Task<bool>> shouldBeRunningAsyncCheck)
        {
            ShouldBeRunningAsyncCheck = shouldBeRunningAsyncCheck;
            return this;
        }
    }
}