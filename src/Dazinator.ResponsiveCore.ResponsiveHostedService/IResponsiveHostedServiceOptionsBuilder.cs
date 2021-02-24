using System;

namespace Dazinator.ResponsiveCore.ResponsiveHostedService
{
    public interface IResponsiveHostedServiceOptionsBuilder<THostedService> : IResponsiveHostedServiceOptionsBuilder
    {
        ResponsiveHostedServiceOptions SetServiceFactory(Func<IServiceProvider, THostedService> resolver);
        ResponsiveHostedServiceOptions SetServiceFactory(Func<THostedService> resolver);
    }

    public interface IResponsiveHostedServiceOptionsBuilder
    {
        ResponsiveHostedServiceOptions ServiceOptions { get; }
        IServiceProvider Services { get; }

    }
}