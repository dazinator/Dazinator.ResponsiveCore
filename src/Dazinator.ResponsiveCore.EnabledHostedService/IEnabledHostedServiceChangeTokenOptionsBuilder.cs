using System;
using System.Linq.Expressions;
using Microsoft.Extensions.Primitives;

namespace Microsoft.Extensions.DependencyInjection
{
    public interface IEnabledHostedServiceChangeTokenOptionsBuilder
    {
        IEnabledHostedServiceChangeTokenOptionsBuilder UseChangeTokenFactory(Func<IServiceProvider, Func<IChangeToken>> resolver);
        IEnabledHostedServiceChangeTokenOptionsBuilder UseChangeTokenFactory(Func<IChangeToken> resolver);
        IEnabledHostedServiceChangeTokenOptionsBuilder UseChangeTokenFactory(Action<ChangeTokenProducerBuilder> configure);
        IEnabledHostedServiceChangeTokenOptionsBuilder UseChangeTokenFactory(Action<IServiceProvider, ChangeTokenProducerBuilder> configure);

        IEnabledHostedServiceChangeTokenOptionsBuilder UseEnabledChecker(Func<IServiceProvider, Func<bool>> resolver);
        IEnabledHostedServiceChangeTokenOptionsBuilder UseEnabledChecker(Action<IFuncBoolBuilderInitial> builder);
        IEnabledHostedServiceChangeTokenOptionsBuilder UseEnabledChecker(Action<IServiceProvider, IFuncBoolBuilderInitial> builder);
        IEnabledHostedServiceChangeTokenOptionsBuilder UseEnabledChecker(Func<bool> resolver);

    }
}