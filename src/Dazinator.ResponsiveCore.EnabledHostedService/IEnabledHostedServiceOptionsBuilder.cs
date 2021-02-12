using System;
using Microsoft.Extensions.Primitives;

namespace Microsoft.Extensions.DependencyInjection
{
    public interface IEnabledHostedServiceOptionsBuilder<THostedService>
    {
        IEnabledHostedServiceOptionsBuilder<THostedService> UseChangeTokenFactory(Func<IServiceProvider, Func<IChangeToken>> resolver);
        IEnabledHostedServiceOptionsBuilder<THostedService> UseChangeTokenFactory(Func<IChangeToken> resolver);
        IEnabledHostedServiceOptionsBuilder<THostedService> UseEnabledChecker(Func<IServiceProvider, Func<bool>> resolver);
        IEnabledHostedServiceOptionsBuilder<THostedService> UseEnabledChecker(Func<bool> resolver);
        IEnabledHostedServiceOptionsBuilder<THostedService> UseServiceFactory(Func<IServiceProvider, THostedService> resolver);
        IEnabledHostedServiceOptionsBuilder<THostedService> UseServiceFactory(Func<THostedService> resolver);

    }
}