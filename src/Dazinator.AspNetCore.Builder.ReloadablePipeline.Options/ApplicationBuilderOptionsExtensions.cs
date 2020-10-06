using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Microsoft.AspNetCore.Builder
{
    public static class ApplicationBuilderOptionsExtensions
    {
        public static IApplicationBuilder UseReloadablePipeline<TOptions>(this IApplicationBuilder builder,
            Action<IApplicationBuilder, TOptions> configure)
        where TOptions : class
        {
            return AddReloadablePipeline(builder, configure, false);
        }

        public static IApplicationBuilder RunReloadablePipeline<TOptions>(this IApplicationBuilder builder, Action<IApplicationBuilder, TOptions> configure)
      where TOptions : class
        {
            return AddReloadablePipeline(builder, configure, true);
        }

        /// <summary>
        /// Adds <see cref="ReloadPipelineMiddleware"/> to the middleware pipeline, with a change token to invalidate it and rebuild it whenever <typeparamref name="TOptions"/> changes.
        /// </summary>
        /// <typeparam name="TOptions"></typeparam>
        /// <param name="builder"></param>
        /// <param name="configure"></param>
        /// <param name="isTerminal"></param>
        /// <returns></returns>
        /// <remarks>You must ensure <typeparamref name="TOptions"/></remarks> has been registered with the options system via services.Configure{TOptions} in ConfigureServices.
        private static IApplicationBuilder AddReloadablePipeline<TOptions>(this IApplicationBuilder builder, Action<IApplicationBuilder, TOptions> configure, bool isTerminal)
    where TOptions : class
        {

            var monitor = builder.ApplicationServices.GetRequiredService<IOptionsMonitor<TOptions>>();
            return CallbackRegistrationExtensions.AddReloadablePipelineMiddleware(builder, (onChangedCallback) =>
            {
                return monitor.OnChange(a => onChangedCallback());
            }, (b) =>
            {
                configure(b, monitor.CurrentValue);
            }, isTerminal);
        }
    }
}