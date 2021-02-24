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
            services.AddHostedService<EnabledHostedServiceAsync<TService>>((sp) =>
#else
            services.AddTransient<IHostedService, EnabledHostedServiceAsync<TService>>((sp) =>
#endif
            {
                var builder = new EnabledHostedServiceOptionsBuilder<TService>(sp);
                configure(builder);

                var innerService = builder.ServiceResolver(sp);              
                return new EnabledHostedServiceAsync<TService>(innerService, builder.ServiceOptions);
            });
            return services;
        }

    }


}