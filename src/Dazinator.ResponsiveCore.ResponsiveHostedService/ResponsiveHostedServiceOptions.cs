using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;

namespace Dazinator.ResponsiveCore.ResponsiveHostedService
{

    public class ResponsiveHostedServiceOptions
    {
        public ResponsiveHostedServiceOptions(Func<IChangeToken> changeTokenProducer,
            IDisposable changeTokenProducerLifetime,
            Func<CancellationToken, Task<bool>> shouldBeRunningAsyncCheck)
        {
            if (changeTokenProducer != null)
            {
                ChangeTokenProducer = changeTokenProducer;
            }
            else
            {
                ChangeTokenProducer = () => EmptyChangeToken.Instance;
            }

            ChangeTokenProducerLifetime = changeTokenProducerLifetime;

            if (shouldBeRunningAsyncCheck != null)
            {
                ShouldBeRunningAsyncCheck = shouldBeRunningAsyncCheck;
            }
            else
            {
                ShouldBeRunningAsyncCheck = (c) => Task.FromResult(true);
            }

        }
        public Func<IChangeToken> ChangeTokenProducer { get; }
        public IDisposable ChangeTokenProducerLifetime { get; }
        public Func<CancellationToken, Task<bool>> ShouldBeRunningAsyncCheck { get; }
        public int DebounceDelayInMilliseconds { get; set; } = 500;

    }
}