using System;
using Dazinator.Extensions.Hosting.EnabledHostedService;
using Microsoft.Extensions.DependencyInjection;
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
        /// < param name= "changeTokenFactory" > Factory that produces <see cref = "IChangeToken" /></ param > s, used to signal the service to re-check it's latest enabled / disabled state via the <param name="shouldBeRunning"/> callback.
        /// <param name = "shouldBeRunning" > A delegate that returns whether the service is currently enabled or not. 
        /// If the service is enabled but not currently running it will be started.
        /// If it is not enabled but is currently running it will be stopped.
        /// </param>
        /// <returns></returns>
        public static IServiceCollection AddEnabledHostedService<TService>(
            this IServiceCollection services,
            Func<IServiceProvider, Func<IChangeToken>> getChangeTokenFactory,
            Func<IServiceProvider, bool> shouldBeRunning)
            where TService : IHostedService
        {
            return AddEnabledHostedService<TService>(services,
                (sp) => ActivatorUtilities.CreateInstance<TService>(sp),
                getChangeTokenFactory,
                shouldBeRunning);
        }

        public static IServiceCollection AddEnabledHostedService<TService>(
            this IServiceCollection services,
            Func<IServiceProvider, TService> innerServiceFactory,
            Func<IServiceProvider, Func<IChangeToken>> getChangeTokenFactory,
            Func<IServiceProvider, bool> shouldBeRunning)
            where TService : IHostedService
        {
#if SUPPORTS_ADD_HOSTED_SERVICE_WITHFACTORYFUNC
            services.AddHostedService<EnabledHostedService<TService>>((sp) =>
#else
            services.AddTransient<IHostedService, EnabledHostedService<TService>>((sp) =>
#endif
            {
                var inner = innerServiceFactory(sp);
                var tokenFactory = getChangeTokenFactory.Invoke(sp);
                return new EnabledHostedService<TService>(inner,
                    tokenFactory,
                    () => shouldBeRunning(sp));
            });
            return services;
        }


        public static IServiceCollection AddEnabledHostedService<TService>(
           this IServiceCollection services,
           Func<IServiceProvider, TService> innerServiceFactory,
           Func<IChangeToken> getChangeTokenFactory,
           Func<IServiceProvider, bool> shouldBeRunning)
           where TService : IHostedService
        {
            return AddEnabledHostedService<TService>(services, innerServiceFactory, (sp) => getChangeTokenFactory, shouldBeRunning);
        }



        public static IServiceCollection AddEnabledHostedService<TService>(
          this IServiceCollection services,
          Func<IServiceProvider, TService> innerServiceFactory,
          Func<IChangeToken> getChangeTokenFactory,
          Func<bool> shouldBeRunning)
          where TService : IHostedService
        {
            return AddEnabledHostedService<TService>(services, innerServiceFactory, (sp) => getChangeTokenFactory, (sp) => shouldBeRunning?.Invoke() ?? true);
        }

    }


}