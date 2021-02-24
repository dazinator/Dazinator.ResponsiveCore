using System;

namespace Microsoft.Extensions.DependencyInjection
{
    public class EnabledHostedServiceOptionsBuilder<THostedService> : IEnabledHostedServiceOptionsBuilder<THostedService>
    {
        public EnabledHostedServiceOptionsBuilder(IServiceProvider services)
        {
            Services = services;
            ServiceResolver = ActivatorUtilities.GetServiceOrCreateInstance<THostedService>;
            ServiceOptions = new EnabledHostedServiceOptions(Services);
            // ChangeTokenFactoryResolver = (sp) => () => EmptyChangeToken.Instance;
        }

        public Func<IServiceProvider, THostedService> ServiceResolver { get; set; }
        public EnabledHostedServiceOptions ServiceOptions { get; set; }
        public IServiceProvider Services { get; }

        /// <summary>
        /// Use a factory Func to create the instance of the IHosted service that will be controlled by the enabled / disabled check. If you don't set this, by default <see cref="ActivatorUtilities.GetServiceOrCreateInstance<typeparamref name="THostedService"/>"/> is used.
        /// </summary>
        /// <param name="resolver"></param>
        /// <returns></returns>
        public EnabledHostedServiceOptions SetServiceFactory(Func<IServiceProvider, THostedService> resolver)
        {
            ServiceResolver = resolver;
            return ServiceOptions;
        }

        /// <summary>
        /// Use a factory Func to create the instance of the IHosted service that will be controlled by the enabled / disabled check. If you don't set this, by default <see cref="ActivatorUtilities.GetServiceOrCreateInstance<typeparamref name="THostedService"/>"/> is used.
        /// </summary>
        /// <param name="resolver"></param>
        /// <returns></returns>
        public EnabledHostedServiceOptions SetServiceFactory(Func<THostedService> resolver)
        {
            ServiceResolver = (s) => resolver();
            return ServiceOptions;
        }

    }


}