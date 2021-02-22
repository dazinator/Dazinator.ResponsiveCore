using System;

namespace Microsoft.Extensions.DependencyInjection
{
    public interface IEnabledHostedServiceOptionsBuilder<THostedService> : IEnabledHostedServiceChangeTokenOptionsBuilder
    {
        IEnabledHostedServiceOptionsBuilder<THostedService> UseServiceFactory(Func<IServiceProvider, THostedService> resolver);
        IEnabledHostedServiceOptionsBuilder<THostedService> UseServiceFactory(Func<THostedService> resolver);
    }
}