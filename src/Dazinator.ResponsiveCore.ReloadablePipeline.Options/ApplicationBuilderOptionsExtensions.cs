using System;
using Dazinator.ResponsiveCore.ReloadablePipeline;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Microsoft.AspNetCore.Builder
{
    public static class ApplicationBuilderOptionsExtensions
    {
        /// <summary>
        /// Use a pipeline that will reload when an Options change is detected via <see cref="IOptionsMonitor{TOptions}"/>
        /// </summary>
        /// <typeparam name="TOptions"></typeparam>
        /// <param name="builder"></param>
        /// <param name="configure"></param>
        /// <param name="rebuildStrategy">The strategy to use for rebuilding the middleware pipeline. <see cref="RebuildOnDemandStrategy"/></param> and also <see cref="RebuildOnInvalidateStrategy"/> for examples. If null, <see cref="DefaultRebuildStrategy.Create"/> will be used.
        /// <returns></returns>
        public static IApplicationBuilder UseReloadablePipeline<TOptions>(this IApplicationBuilder builder,
            Action<IApplicationBuilder, TOptions> configure, IRebuildStrategy rebuildStrategy = null)
        where TOptions : class
        {
            return AddReloadablePipeline(builder, configure, false, rebuildStrategy);
        }

        /// <summary>
        /// Use a pipeline that will reload when an Options change is detected via <see cref="IOptionsMonitor{TOptions}"/>
        /// </summary>
        /// <typeparam name="TOptions"></typeparam>
        /// <param name="builder"></param>
        /// <param name="configure"></param>
        /// <param name="rebuildStrategy">The strategy to use for rebuilding the middleware pipeline. <see cref="RebuildOnDemandStrategy"/></param> and also <see cref="RebuildOnInvalidateStrategy"/> for examples. If null, <see cref="DefaultRebuildStrategy.Create"/> will be used.
        /// <returns></returns>
        public static IApplicationBuilder RunReloadablePipeline<TOptions>(this IApplicationBuilder builder,
            Action<IApplicationBuilder, TOptions> configure, IRebuildStrategy rebuildStrategy = null)
      where TOptions : class
        {
            return AddReloadablePipeline(builder, configure, true, rebuildStrategy);
        }

        /// <summary>
        /// Adds <see cref="ReloadPipelineMiddleware"/> to the middleware pipeline, with a change token to invalidate it and rebuild it whenever <typeparamref name="TOptions"/> changes.
        /// </summary>
        /// <typeparam name="TOptions"></typeparam>
        /// <param name="builder"></param>
        /// <param name="configure"></param>
        /// <param name="isTerminal"></param>
        /// <param name="rebuildStrategy">The strategy to use for rebuilding the middleware pipeline. <see cref="RebuildOnDemandStrategy"/></param> and also <see cref="RebuildOnInvalidateStrategy"/> for examples. If null, <see cref="DefaultRebuildStrategy.Create"/> will be used.
        /// <returns></returns>
        /// <remarks>You must ensure <typeparamref name="TOptions"/></remarks> has been registered with the options system via services.Configure{TOptions} in ConfigureServices.
        private static IApplicationBuilder AddReloadablePipeline<TOptions>(this IApplicationBuilder builder, Action<IApplicationBuilder, TOptions> configure, bool isTerminal, IRebuildStrategy rebuildStrategy)
    where TOptions : class
        {

            var monitor = builder.ApplicationServices.GetRequiredService<IOptionsMonitor<TOptions>>();
            return CallbackRegistrationExtensions.AddReloadablePipelineMiddleware(builder, (onChangedCallback) =>
            {
                return monitor.OnChange(a => onChangedCallback());
            }, (b) =>
            {
                configure(b, monitor.CurrentValue);
            }, isTerminal, rebuildStrategy);
        }
    }
}