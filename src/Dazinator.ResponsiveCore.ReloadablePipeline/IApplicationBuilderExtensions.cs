using System;
using Dazinator.ResponsiveCore.ReloadablePipeline;
using Microsoft.Extensions.Primitives;

namespace Microsoft.AspNetCore.Builder
{
    public static class IApplicationBuilderExtensions
    {

        /// <summary>
        /// Uses <see cref="ReloadPipelineMiddleware"/> in the pipeline which allows you to build a section of pipeline that can be optionally invalidated & reloaded at runtime when a supplied <see cref="IChangeToken"/> is signalled.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="changeTokenFactory">Function that is used to obtain an <see cref="IChangeToken"/></param> that will signal when the pipeline should be reloaded.
        /// <param name="configure"></param>
        /// <param name="rebuildStrategy">The strategy to use for rebuilding the middleware pipeline. <see cref="RebuildOnDemandStrategy"/></param> and also <see cref="RebuildOnInvalidateStrategy"/> for examples. If null, <see cref="DefaultRebuildStrategy.Create"/> will be used.
        /// <returns></returns>
        public static IApplicationBuilder UseReloadablePipeline(
            this IApplicationBuilder builder,
           Action<ReloadableMiddlewarePipelineOptions> configureOptions)
        {
            return AddReloadablePipelineMiddleware(builder, (o) =>
            {
                o.IsTerminal = false;
                configureOptions?.Invoke(o);
            });
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
          Action<ReloadableMiddlewarePipelineOptions> configureOptions)
        {
            return AddReloadablePipelineMiddleware(builder, (o) =>
            {
                o.IsTerminal = true;
                configureOptions?.Invoke(o);
            });
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
        public static IApplicationBuilder AddReloadablePipelineMiddleware(
            this IApplicationBuilder builder,
            Action<ReloadableMiddlewarePipelineOptions> configureOptions)
        {
            var options = new ReloadableMiddlewarePipelineOptions(builder);
            configureOptions(options);

            var factory = new RequestDelegateFactory(options, (onNext) =>
            {
                return RequestDelegateUtils.BuildRequestDelegate(builder, onNext, options.ConfigureMiddlewarePipelineDelegate, options.IsTerminal);
            });
            builder.UseMiddleware<ReloadPipelineMiddleware>(factory);
            return builder;
        }

    }

    public class ReloadableMiddlewarePipelineOptions
    {
        public ReloadableMiddlewarePipelineOptions(IApplicationBuilder builder)
        {
            Builder = builder;
            ConfigureMiddlewarePipelineDelegate = null;
            RebuildStrategy = DefaultRebuildStrategy.Create();
        }

        public IRebuildStrategy RebuildStrategy { get; set; } = null;

        public Func<IChangeToken> ChangeTokenProducer { get; set; } = null;
        public IDisposable ChangeTokenProducerLifetime { get; set; } = null;

        public bool IsTerminal { get; set; } = false;

        public IApplicationBuilder Builder { get; }

        public Action<IApplicationBuilder> ConfigureMiddlewarePipelineDelegate { get; set; }

        public ReloadableMiddlewarePipelineOptions RespondsTo(Func<IChangeToken> resolver, IDisposable lifetime)
        {
            ChangeTokenProducer = resolver;
            ChangeTokenProducerLifetime = lifetime;
            return this;
        }

        public ReloadableMiddlewarePipelineOptions SetTerminal()
        {
            IsTerminal = true;
            return this;
        }

        public ReloadableMiddlewarePipelineOptions WithPipelineRebuild(Action<IApplicationBuilder> configure)
        {
            ConfigureMiddlewarePipelineDelegate = configure;
            return this;
        }


    }
}