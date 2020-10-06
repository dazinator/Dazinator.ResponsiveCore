using System;
using System.Threading;
using Dazinator.AspNetCore.Builder.ReloadablePipeline;

namespace Microsoft.AspNetCore.Builder
{
    /// <summary>
    /// Provides addtional extension methods that build on top of <see cref="ChangeTokenExtensions"/> and provide support for using <see cref="CancellationToken"/>s to signal reload.
    /// </summary>
    public static class CancellationTokenExtensions
    {

        /// <summary>
        /// Uses <see cref="ReloadPipelineMiddleware"/> in the pipeline which allows you to trigger reload of a section of pipeline at runtime based on a <see cref="CancellationToken"/> being supplied and signalled.
        /// </summary>
        /// <param name="builder"></param>
        ///<param name="getCancellationToken">Function that returns a cancellation token that will be signalled when the pipeline needs to be reloaded.</param>
        /// <param name="configure"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseReloadablePipeline(this IApplicationBuilder builder,
            Func<CancellationToken> getCancellationToken,
            Action<IApplicationBuilder> configure)
        {
            return AddReloadablePipelineMiddleware(builder, getCancellationToken, configure, false);
        }

        /// <summary>
        /// Runs <see cref="ReloadPipelineMiddleware"/> in the pipeline which allows you to trigger reload of a section of pipeline at runtime based on a <see cref="CancellationToken"/> being supplied and signalled.
        /// </summary>
        /// <param name="builder"></param>
        ///<param name="getCancellationToken">Function that returns a cancellation token that will be signalled when the pipeline needs to be reloaded.</param>
        /// <param name="configure"></param>
        /// <returns></returns>
        public static IApplicationBuilder RunReloadablePipeline(this IApplicationBuilder builder,
          Func<CancellationToken> getCancellationToken,
          Action<IApplicationBuilder> configure)
        {
            return AddReloadablePipelineMiddleware(builder, getCancellationToken, configure, true);
        }



        /// <summary>
        /// Adds <see cref="ReloadPipelineMiddleware"/> to the middleware pipeline, with a factory method that can supply a <see cref="CancellationToken"/> that can be used to invalidate it and cause a reload.
        /// </summary>
        /// <param name="builder"></param>
        ///<param name="getCancellationToken">Function that returns a cancellation token that will be signalled when the pipeline needs to be reloaded.</param>
        /// <param name="configure"></param>
        /// <param name="isTerminal"></param>
        /// <returns></returns>
        public static IApplicationBuilder AddReloadablePipelineMiddleware(this IApplicationBuilder builder,
            Func<CancellationToken> getCancellationToken,
            Action<IApplicationBuilder> configure,
            bool isTerminal)
        {
            var changeTokenFactory = ChangeTokenFactoryHelper.UseCancellationTokens(getCancellationToken);
            return ChangeTokenExtensions.AddReloadablePipelineMiddleware(builder, changeTokenFactory, configure, isTerminal);
        }

    }
}
