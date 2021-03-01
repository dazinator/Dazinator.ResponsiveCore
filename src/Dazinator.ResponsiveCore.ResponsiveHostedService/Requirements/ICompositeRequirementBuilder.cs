using System;
using System.Threading;

namespace Dazinator.ResponsiveCore.ResponsiveHostedService.Requirements
{
    public interface ICompositeRequirementBuilder
    {
        CompositeRequirementBuilder Include<TCheck>() where TCheck : IRequirement;
        CompositeRequirementBuilder Include<TCheck>(Func<IServiceProvider, TCheck> factory) where TCheck : IRequirement;
        CompositeRequirementBuilder IncludeFunc(Func<CancellationToken, bool> check);
        CompositeRequirementBuilder IncludeFunc(Func<CancellationToken, IServiceProvider, bool> check);
    }
}