using System;
using Microsoft.Extensions.Primitives;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class EnabledHostedServiceOptionsExtensions
    {

        #region Options Pattern
        /// <summary>
        /// Adds a hosted service that can responds to <see cref="IOptionsMonitor{TOptions}"/> change 
        /// notifications and can start or stop itself based on the currently configured enablement for the service.
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <typeparam name="TOptions"></typeparam>
        /// <param name="services"></param>
        /// <param name="shouldBeRunning"></param>
        /// <param name="optionsName"></param>
        /// <returns></returns>
        public static IServiceCollection AddOptionsEnabledHostedService<TService, TOptions>(
           this IServiceCollection services,
           Predicate<TOptions> shouldBeRunning, string optionsName = default)
           where TService : IHostedService
        {

            return services.AddEnabledHostedService<TService>(
                (sp) =>
                {
                    var monitor = sp.GetRequiredService<IOptionsMonitor<TOptions>>();
                    var changeTokenFactory = ChangeTokenFactoryHelper.UseCallbackRegistrations((onChangedCallback) =>
                    {
                        return monitor.OnChange(a => onChangedCallback());
                    });
                    return changeTokenFactory;
                },
                (sp) =>
                {
                    var monitor = sp.GetRequiredService<IOptionsMonitor<TOptions>>();
                    if (string.IsNullOrWhiteSpace(optionsName))
                    {
                        return shouldBeRunning(monitor.CurrentValue);
                    }
                    else
                    {
                        return shouldBeRunning(monitor.Get(optionsName));
                    }
                });
        }

        /// <summary>
        /// Adds a hosted service that can responds to <see cref="IOptionsMonitor{TOptions}"/> change 
        /// notifications and can start or stop itself based on the currently configured enablement for the service.
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <typeparam name="TOptions"></typeparam>
        /// <param name="services"></param>
        /// <param name="innerServiceFactory"></param>
        /// <param name="shouldBeRunning"></param>
        /// <param name="optionsName"></param>
        /// <returns></returns>
        public static IServiceCollection AddOptionsEnabledHostedService<TService, TOptions>(
          this IServiceCollection services,
          Func<IServiceProvider, TService> innerServiceFactory,
          Predicate<TOptions> shouldBeRunning, string optionsName = default)
          where TService : IHostedService
        {

            return services.AddEnabledHostedService<TService>(
                innerServiceFactory,
                (sp) =>
                {
                    var monitor = sp.GetRequiredService<IOptionsMonitor<TOptions>>();
                    var changeTokenFactory = ChangeTokenFactoryHelper.UseCallbackRegistrations((onChangedCallback) =>
                    {
                        return monitor.OnChange(a => onChangedCallback());
                    });
                    return changeTokenFactory;
                },
                (sp) =>
                {
                    var monitor = sp.GetRequiredService<IOptionsMonitor<TOptions>>();
                    if (string.IsNullOrWhiteSpace(optionsName))
                    {
                        return shouldBeRunning(monitor.CurrentValue);
                    }
                    else
                    {
                        return shouldBeRunning(monitor.Get(optionsName));
                    }
                });
        }
        #endregion Options Pattern
 
    }
}
