using System;
using Dazinator.ResponsiveCore.ReloadablePipeline;
using Microsoft.Extensions.Primitives;

namespace Microsoft.AspNetCore.Builder
{
    public static class ChangeTokenExtensions
    {

        /// <summary>
        /// Uses <see cref="ReloadPipelineMiddleware"/> in the pipeline which allows you to build a section of pipeline that can be optionally invalidated & reloaded at runtime when a supplied <see cref="IChangeToken"/> is signalled.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="changeTokenFactory">Function that is used to obtain an <see cref="IChangeToken"/></param> that will signal when the pipeline should be reloaded.
        /// <param name="configure"></param>
        /// <param name="rebuildStrategy">The strategy to use for rebuilding the middleware pipeline. <see cref="RebuildOnDemandStrategy"/></param> and also <see cref="RebuildOnInvalidateStrategy"/> for examples. If null, <see cref="DefaultRebuildStrategy.Create"/> will be used.
        /// <returns></returns>
        public static IApplicationBuilder UseReloadablePipeline(this IApplicationBuilder builder,
           Func<IChangeToken> changeTokenFactory,
           Action<IApplicationBuilder> configure, IRebuildStrategy rebuildStrategy = null)
        {
            return AddReloadablePipelineMiddleware(builder, changeTokenFactory, configure, false, rebuildStrategy);
        }

        /// <summary>
        /// Runs <see cref="ReloadPipelineMiddleware"/> in the pipeline which allows you to build a section of pipeline that can be optionally invalidated & reloaded at runtime when a supplied <see cref="IChangeToken"/> is signalled.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="changeTokenFactory">Function that is used to obtain an <see cref="IChangeToken"/></param> that will signal when the pipeline should be reloaded.
        /// <param name="configure"></param>
        /// <param name="rebuildStrategy">The strategy to use for rebuilding the middleware pipeline. <see cref="RebuildOnDemandStrategy"/></param> and also <see cref="RebuildOnInvalidateStrategy"/> for examples. If null, <see cref="DefaultRebuildStrategy.Create"/> will be used.
        /// <returns></returns>
        public static IApplicationBuilder RunReloadablePipeline(this IApplicationBuilder builder,
          Func<IChangeToken> changeTokenFactory,
          Action<IApplicationBuilder> configure, IRebuildStrategy rebuildStrategy = null)
        {
            return AddReloadablePipelineMiddleware(builder, changeTokenFactory, configure, true, rebuildStrategy);
        }


        /// <summary>
        /// Adds <see cref="ReloadPipelineMiddleware"/> to the middleware pipeline, with a method to obtain a change token used to be notified that the pipleine should be reloaded.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="changeTokenFactory">Function that is used to obtain an <see cref="IChangeToken"/></param> that will signal when the pipeline should be reloaded.
        /// <param name="configure"></param>
        /// <param name="isTerminal"></param>
        /// <param name="rebuildStrategy">The strategy to use for rebuilding the middleware pipeline. <see cref="RebuildOnDemandStrategy"/></param> and also <see cref="RebuildOnInvalidateStrategy"/> for examples. If null, <see cref="DefaultRebuildStrategy.Create"/> will be used.
        /// <returns></returns>
        public static IApplicationBuilder AddReloadablePipelineMiddleware(this IApplicationBuilder builder,
            Func<IChangeToken> changeTokenFactory,
            Action<IApplicationBuilder> configure,
            bool isTerminal,
            IRebuildStrategy rebuildStrategy = null)
        {

            if (rebuildStrategy == null)
            {
                rebuildStrategy = DefaultRebuildStrategy.Create();
            }

            var factory = new RequestDelegateFactory(changeTokenFactory, rebuildStrategy, (onNext) =>
            {
                return RequestDelegateUtils.BuildRequestDelegate(builder, onNext, configure, isTerminal);
            });
            builder.UseMiddleware<ReloadPipelineMiddleware>(factory);
            return builder;
        }

    }
}