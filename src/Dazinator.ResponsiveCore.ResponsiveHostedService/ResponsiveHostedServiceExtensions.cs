using System;
using Dazinator.ResponsiveCore.ResponsiveHostedService;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Primitives;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ResponsiveHostedServiceExtensions
    {

        /// <summary>
        /// Register an<see cref= "IHostedService" /> that responds to signalled changed tokens, by starting or stopping accorsignly based on a status check.
        /// </summary>
        /// <typeparam name = "TService" ></ typeparam >
        /// < param name= "services" ></ param >       
        /// <returns></returns>
        public static IServiceCollection AddResponsiveHostedService<TService>(
           this IServiceCollection services,
           Action<IResponsiveHostedServiceOptionsBuilder<TService>> configure)
           where TService : IHostedService
        {
#if SUPPORTS_ADD_HOSTED_SERVICE_WITHFACTORYFUNC
            services.AddHostedService<ResponsiveHostedServiceAsync<TService>>((sp) =>
#else
            services.AddTransient<IHostedService, ResponsiveHostedServiceAsync<TService>>((sp) =>
#endif
            {
                var builder = new ResponsiveHostedServiceOptionsBuilder<TService>(sp);
                configure(builder);

                var innerService = builder.ServiceResolver(sp);
                return new ResponsiveHostedServiceAsync<TService>(innerService, builder.Build());
            });
            return services;
        }

    }


}