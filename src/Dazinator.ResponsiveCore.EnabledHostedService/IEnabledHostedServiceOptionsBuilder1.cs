using System;

namespace Microsoft.Extensions.DependencyInjection
{
    public interface IEnabledHostedServiceOptionsBuilder
    {
        EnabledHostedServiceOptions ServiceOptions { get; }
        IServiceProvider Services { get; }

    }
}