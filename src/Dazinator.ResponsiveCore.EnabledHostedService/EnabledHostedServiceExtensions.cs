using System;
using Dazinator.ResponsiveCore.EnabledHostedService;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Primitives;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class EnabledHostedServiceExtensions
    {

        /// <summary>
        /// Register an<see cref= "IHostedService" /> that can be started and stopped at runtime by signalling an <see cref = "IChangeToken" /> to apply the current enabled / disabled state. This state is obtained via a callback that will be invoked when the change token is signalled.
        /// </summary>
        /// <typeparam name = "TService" ></ typeparam >
        /// < param name= "services" ></ param >       
        /// <returns></returns>
        public static IServiceCollection AddEnabledHostedService<TService>(
           this IServiceCollection services,
           Action<IEnabledHostedServiceOptionsBuilder<TService>> configure)
           where TService : IHostedService
        {
#if SUPPORTS_ADD_HOSTED_SERVICE_WITHFACTORYFUNC
            services.AddHostedService<EnabledHostedService<TService>>((sp) =>
#else
            services.AddTransient<IHostedService, EnabledHostedService<TService>>((sp) =>
#endif
            {
                var builder = new EnabledHostedServiceOptionsBuilder<TService>();
                configure(builder);

                var innerService = builder.ServiceResolver(sp);
                var tokenFactory = builder.ChangeTokenFactoryResolver?.Invoke(sp);
                var isEnabledDelegate = builder.IsEnabledDelegateResolver?.Invoke(sp);

                return new EnabledHostedService<TService>(innerService,
                    tokenFactory,
                    isEnabledDelegate);
            });
            return services;
        }

    }


}