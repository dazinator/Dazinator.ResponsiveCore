using System;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;

namespace Microsoft.Extensions.DependencyInjection
{

    public static class EnabledHostedServiceOptionsExtensions
    {

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
        public static IEnabledHostedServiceOptionsBuilder<TService> UseOptionsMonitor<TService, TOptions>(
           this IEnabledHostedServiceOptionsBuilder<TService> options,
           Predicate<TOptions> shouldBeRunning,
           string optionsName = default)
           where TService : IHostedService
        {
            options.UseChangeTokenFactory((sp, builder) =>
            {
                var monitor = sp.GetRequiredService<IOptionsMonitor<TOptions>>();
                builder.IncludeOptions(monitor);
            })
            .UseEnabledChecker(sp =>
            {
                var monitor = sp.GetRequiredService<IOptionsMonitor<TOptions>>();
                Func<bool> result = () =>
                {
                    TOptions options;
                    if (string.IsNullOrWhiteSpace(optionsName))
                    {
                        options = monitor.CurrentValue;
                    }
                    else
                    {
                        options = monitor.Get(optionsName);
                    }
                    return shouldBeRunning(options);
                };
                return result;
            });
            return options;
        }

        /// <summary>
        /// Configures a predicate to be used to determine if the service should be running or not based on value of <see cref="{TOptions}"/>
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <typeparam name="TOptions"></typeparam>
        /// <param name="options"></param>
        /// <param name="shouldBeRunning"></param>
        /// <param name="optionsName"></param>
        /// <returns></returns>
        public static IEnabledHostedServiceOptionsBuilder<TService> UseOptionsEnabledChecker<TService, TOptions>(
          this IEnabledHostedServiceOptionsBuilder<TService> options,
          Predicate<TOptions> shouldBeRunning,
          string optionsName = default)
          where TService : IHostedService
        {
            options.UseEnabledChecker(sp =>
            {
                var monitor = sp.GetRequiredService<IOptionsMonitor<TOptions>>();
                Func<bool> result = () =>
                {
                    TOptions options;
                    if (string.IsNullOrWhiteSpace(optionsName))
                    {
                        options = monitor.CurrentValue;
                    }
                    else
                    {
                        options = monitor.Get(optionsName);
                    }
                    return shouldBeRunning(options);
                };
                return result;
            });
            return options;
        }

    }


}