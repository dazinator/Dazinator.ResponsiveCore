using System;
using Dazinator.AspNetCore.Builder.ReloadablePipeline;

namespace Microsoft.AspNetCore.Builder
{

    /// <summary>
    /// Provides extension methods that build on top of <see cref="ChangeTokenExtensions"/> and provide support for using callback registerations to signal reload.
    /// </summary>
    public static class CallbackRegistrationExtensions
    {
        /// <summary>
        /// Uses <see cref="ReloadPipelineMiddleware"/> in the pipeline which allows you to build a section of pipeline that can be optionally invalidated & reloaded at runtime.
        /// </summary>
        /// <param name="builder"></param>
        ///<param name="registerListener">Function that will be invoked to register a callback, that should then be invoked whenever a change occurs that should trigger a reload. It should return an <see cref="IDisposable"/> that will remove the registration when the caller disposes of it, in order to stop being notifified / destroy the subscription.</param>
        /// <param name="configure"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseReloadablePipeline(this IApplicationBuilder builder,
            Func<Action, IDisposable> registerListener,
            Action<IApplicationBuilder> configure)
        {
            return AddReloadablePipelineMiddleware(builder, registerListener, configure, false);
        }

        /// <summary>
        /// Runs <see cref="ReloadPipelineMiddleware"/> in the pipeline which allows you to build a section of pipeline that can be optionally invalidated & reloaded at runtime.
        /// </summary>
        /// <param name="builder"></param>
        ///<param name="registerListener">Function that registers a callback to be invoked when a change occurs, and returns an <see cref="IDisposable"/> that will remove the registration when the caller disposes of it in order to stop being notifified.</param>
        /// <param name="configure"></param>
        /// <returns></returns>
        public static IApplicationBuilder RunReloadablePipeline(this IApplicationBuilder builder,
          Func<Action, IDisposable> registerListener,
          Action<IApplicationBuilder> configure)
        {
            return AddReloadablePipelineMiddleware(builder, registerListener, configure, true);
        }


        /// <summary>
        /// Adds <see cref="ReloadPipelineMiddleware"/> to the middleware pipeline, with a change token to invalidate it and rebuild it.
        /// </summary>
        /// <param name="builder"></param>
        ///<param name="registerListener">Function that registers a callback to be invoked when a change occurs, and returns an <see cref="IDisposable"/> that will remove the registration when the caller disposes of it in order to stop being notifified.</param>
        /// <param name="configure"></param>
        /// <param name="isTerminal"></param>
        /// <returns></returns>
        public static IApplicationBuilder AddReloadablePipelineMiddleware(this IApplicationBuilder builder,
            Func<Action, IDisposable> registerListener,
            Action<IApplicationBuilder> configure,
            bool isTerminal)
        {
            var changeTokenFactory = ChangeTokenFactoryHelper.UseCallbackRegistrations(registerListener);     
            return ChangeTokenExtensions.AddReloadablePipelineMiddleware(builder, changeTokenFactory, configure, isTerminal);
        }
    }
}
