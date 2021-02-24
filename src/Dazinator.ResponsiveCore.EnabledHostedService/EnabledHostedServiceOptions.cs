using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Primitives;

namespace Microsoft.Extensions.DependencyInjection
{

    public class EnabledHostedServiceOptions
    {
        public EnabledHostedServiceOptions(IServiceProvider services)
        {
            Services = services;
        }

        public IServiceProvider Services { get; set; }
        public Func<IChangeToken> ChangeTokenProducer { get; set; } = () => EmptyChangeToken.Instance;
        public IDisposable ChangeTokenProducerLifetime { get; set; } = null;
        public Func<CancellationToken, Task<bool>> ShouldBeRunningAsyncCheck { get; set; } = (c) => Task.FromResult(true);
        public EnabledHostedServiceOptions SetChangeTokenProducer(Func<IChangeToken> resolver, IDisposable lifetime)
        {
            ChangeTokenProducer = resolver;
            ChangeTokenProducerLifetime = lifetime;
            return this;
        }

        public EnabledHostedServiceOptions SetShouldBeRunningCheck(Func<bool> shouldBeRunningCheck)
        {
            ShouldBeRunningAsyncCheck = (cancelToken) =>
                {
                    var result = shouldBeRunningCheck();
                    return Task.FromResult(result);
                };
            return this;
        }

        public EnabledHostedServiceOptions SetShouldBeRunningAsyncCheck(Func<CancellationToken, Task<bool>> shouldBeRunningAsyncCheck)
        {
            ShouldBeRunningAsyncCheck = shouldBeRunningAsyncCheck;
            return this;
        }
    }
}