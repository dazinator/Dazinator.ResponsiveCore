using System;
using Dazinator.ResponsiveCore.ResponsiveHostedService;
using Dazinator.ResponsiveCore.ResponsiveHostedService.Requirements;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ResponsiveServicesRequirementsExtensions
    {
        public static void ConfigureRequirements(
            this ResponsiveHostedServiceOptions options,
            Action<ICompositeRequirementBuilder> configure)
        {
            var builder = new CompositeRequirementBuilder();
            configure(builder);

            options.WithAsyncShouldBeRunningCheck(async (cancelToken) =>
            {
                var composite = builder.Build(options.Services);
                try
                {
                    return await composite.IsSatisfied(cancelToken);
                }
                finally
                {
                    var disposableComposite = composite as IDisposable;
                    disposableComposite?.Dispose();
                }
            });
        }
    }
}