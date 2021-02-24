using System;

namespace Microsoft.Extensions.DependencyInjection
{
    public interface IEnabledHostedServiceOptionsBuilder<THostedService> : IEnabledHostedServiceOptionsBuilder
    {
        EnabledHostedServiceOptions SetServiceFactory(Func<IServiceProvider, THostedService> resolver);
        EnabledHostedServiceOptions SetServiceFactory(Func<THostedService> resolver);
    }
}