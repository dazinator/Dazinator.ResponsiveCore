using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;

namespace Sample
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            // Bind config options for reloadable pipeline
            services.Configure<PipelineOptions>(Configuration.GetSection("Pipeline"));

            // Bind config options for hosted service that can be start / stopped via config change
            services.Configure<HostedServiceOptions>(Configuration.GetSection("HostedService"));

            services.AddResponsiveHostedService<HostedService>(o =>
            {
                var monitor = o.Services.GetRequiredService<IOptionsMonitor<HostedServiceOptions>>();
                var tokenProducer = new ChangeTokenProducerBuilder()
                                   .IncludeSubscribingHandlerTrigger((trigger) => monitor.OnChange((o, n) => trigger()))
                                   .Build(out var disposable);

                o.ServiceOptions.RespondsTo(tokenProducer, disposable)
                                .ShouldBeRunning(() => monitor.CurrentValue?.Enabled ?? false);

            });
        }


        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // Note: UseReloadablePipeline vs RunReloadablePipeline (latter is terminal, former is not).

            // make a change to appsettings.json "Pipelines" section and watch log output in console on furture requests.
            app.UseReloadablePipeline((options) =>
            {
                var monitor = app.ApplicationServices.GetRequiredService<IOptionsMonitor<PipelineOptions>>();
                var tokenProducer = new ChangeTokenProducerBuilder()
                                  .IncludeSubscribingHandlerTrigger((trigger) => monitor.OnChange((o, n) => trigger()))
                                  .Build(out var disposable);

                options.RespondsTo(tokenProducer, disposable)
                       .WithPipelineRebuild((app) =>
                       {
                           ConfigureReloadablePipeline(app, env, monitor.CurrentValue);
                       });
            });

            Action triggerReload = null;

            // Demonstrates another reloadable middleware pipeline that is reloaded by triggering a cancellation token.
            app.Map("/specialpipeline", (builder) =>
            {

                builder.RunReloadablePipeline((options) =>
                {
                    var tokenProducer = new ChangeTokenProducerBuilder()
                                      .IncludeTrigger(out triggerReload)
                                      .Build(out var disposable);

                    options.RespondsTo(tokenProducer, disposable)
                           .WithPipelineRebuild((app) =>
                           {
                               var logger = app.ApplicationServices.GetRequiredService<ILogger<Startup>>();
                               logger.LogInformation("Building special-pipeline!");
                               // this is a terminal pipeline, so let's end with welcome page.
                               app.UseWelcomePage();
                           });
                });
            });

            app.Map("/triggerrebuild", (builder) =>
            {
                builder.Use(async (http, onNext) =>
                {
                    triggerReload?.Invoke();
                    await onNext();
                });

            });

            app.UseWelcomePage();
        }


        private void ConfigureReloadablePipeline(IApplicationBuilder appBuilder, IWebHostEnvironment environment, PipelineOptions options)
        {
            var logger = appBuilder.ApplicationServices.GetRequiredService<ILogger<Startup>>();
            logger.LogInformation("Building reloadable pipeline from current options!");

            if (options.UseDeveloperExceptionPage)
            {
                appBuilder.Use(async (context, onNext) =>
                {
                    var logger = context.RequestServices.GetRequiredService<ILogger<Startup>>();
                    logger.LogInformation("Using dev middleware!");
                    await onNext();
                });
            }
            else
            {
                appBuilder.Use(async (context, onNext) =>
                {
                    var logger = context.RequestServices.GetRequiredService<ILogger<Startup>>();
                    logger.LogInformation("Not using dev middleware..");
                    await onNext();
                });
            }
        }
    }


}