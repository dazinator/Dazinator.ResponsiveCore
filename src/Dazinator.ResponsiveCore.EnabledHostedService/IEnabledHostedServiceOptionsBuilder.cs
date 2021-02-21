using System;
using System.Linq.Expressions;
using Microsoft.Extensions.Primitives;

namespace Microsoft.Extensions.DependencyInjection
{
    public interface IEnabledHostedServiceOptionsBuilder<THostedService>
    {
        IEnabledHostedServiceOptionsBuilder<THostedService> UseChangeTokenFactory(Func<IServiceProvider, Func<IChangeToken>> resolver);
        IEnabledHostedServiceOptionsBuilder<THostedService> UseChangeTokenFactory(Func<IChangeToken> resolver);
        IEnabledHostedServiceOptionsBuilder<THostedService> UseChangeTokenFactory(Action<CompositeChangeTokenFactoryBuilder> configure);
        IEnabledHostedServiceOptionsBuilder<THostedService> UseChangeTokenFactory(Action<IServiceProvider, CompositeChangeTokenFactoryBuilder> configure);
        IEnabledHostedServiceOptionsBuilder<THostedService> UseEnabledChecker(Func<IServiceProvider, Func<bool>> resolver);
        IEnabledHostedServiceOptionsBuilder<THostedService> UseEnabledChecker(Action<IFuncBoolBuilderInitial> builder);
        IEnabledHostedServiceOptionsBuilder<THostedService> UseEnabledChecker(Action<IServiceProvider, IFuncBoolBuilderInitial> builder);
        IEnabledHostedServiceOptionsBuilder<THostedService> UseEnabledChecker(Func<bool> resolver);
        IEnabledHostedServiceOptionsBuilder<THostedService> UseServiceFactory(Func<IServiceProvider, THostedService> resolver);
        IEnabledHostedServiceOptionsBuilder<THostedService> UseServiceFactory(Func<THostedService> resolver);

    }
}