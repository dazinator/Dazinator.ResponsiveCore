using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Primitives;

namespace Dazinator.ResponsiveCore.ResponsiveHostedService
{
    public interface IResponsiveHostedServiceOptionsBuilder<THostedService> : IResponsiveHostedServiceOptionsBuilder
    {
        IResponsiveHostedServiceOptionsBuilder SetServiceFactory(Func<IServiceProvider, THostedService> resolver);
        IResponsiveHostedServiceOptionsBuilder SetServiceFactory(Func<THostedService> resolver);
    }

    public interface IResponsiveHostedServiceOptionsBuilder
    {
        IServiceProvider Services { get; }
        Func<IChangeToken> ChangeTokenProducer { get; set; }
        IDisposable ChangeTokenProducerLifetime { get; set; }
        Func<CancellationToken, Task<bool>> ShouldBeRunningAsyncCheck { get; set; }
        IResponsiveHostedServiceOptionsBuilder RespondsTo(Func<IChangeToken> resolver, IDisposable lifetime, int debounceDelayInMs = ResponsiveHostedServiceOptions.DefaultDebounceDelayInMs);
        void ShouldBeRunning(Func<bool> shouldBeRunningCheck);
        void WithAsyncShouldBeRunningCheck(Func<CancellationToken, Task<bool>> shouldBeRunningAsyncCheck);
    }
}