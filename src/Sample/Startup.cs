using System.Threading;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

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

            services.AddOptionsEnabledHostedService<HostedService, HostedServiceOptions>(
                       shouldBeRunning: options =>
                       {
                           return options.Enabled;
                       });
        }


        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // Note: UseReloadablePipeline vs RunReloadablePipeline (latter is terminal, former is not).

            // make a change to appsettings.json "Pipelines" section and watch log output in console on furture requests.
            app.UseReloadablePipeline<PipelineOptions>((builder, options) => ConfigureReloadablePipeline(builder, env, options));

            // Demonstrates another reloadable middleware pipeline that is reloaded by triggering a cancellation token.
            app.Map("/specialpipeline", (builder) =>
            {
                // Using extension method that allows a cancellation token to be supplied to trigger reload.
                builder.RunReloadablePipeline(() =>
                {
                    var existingCts = SpecialPipelineManualCancellationTokenSource;
                    if (existingCts != null)
                    {
                        existingCts.Dispose();
                        existingCts = null;
                    }

                    var cts = new CancellationTokenSource();
                    // capture reference to it so we can later trigger this pipeline to rebuild.
                    SpecialPipelineManualCancellationTokenSource = cts;
                    return cts.Token;
                }, (subBuilder) =>
                {
                    var logger = subBuilder.ApplicationServices.GetRequiredService<ILogger<Startup>>();
                    logger.LogInformation("Building special-pipeline!");

                    // this is a terminal pipeline, so let's end with welcome page.
                    subBuilder.UseWelcomePage();
                });

            });

            app.Map("/triggerrebuild", (builder) =>
            {
                builder.Use(async (http, onNext) =>
                {
                    SpecialPipelineManualCancellationTokenSource?.Cancel();
                    await onNext();
                });

            });

            app.UseWelcomePage();
        }

        public CancellationTokenSource SpecialPipelineManualCancellationTokenSource { get; set; }


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