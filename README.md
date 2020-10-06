## Features

Build middleware pipelines that can be reloaded at runtime - when a changes are detected.

## Example

1. Add the `Dazinator.AspNetCore.Builder.ReloadablePipeline.Options` nuget package to your project.
2. Configure an `Options` class, and then build a middleware pipeline that from it that will be rebuilt whenever `IOptionsMonitor` detects a change:
 
```csharp

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
            var configSection = Configuration.GetSection("Pipeline");
            services.Configure<PipelineOptions>(configSection);
        }


        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // Note: Use vs Run (latter is terminal, former is not)
            // make a change to appsettings.json "Pipelines" section and watch log output in console on furture requests.
            app.UseReloadablePipeline<PipelineOptions>((builder, options) => ConfigureReloadablePipeline(builder, env, options));
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


```

## How do I signal the pipeline to rebuild for other sorts of changes - for example a button click?

The extension methods provided in `Dazinator.AspNetCore.Builder.ReloadablePipeline.Options` package is just a thin wrapper around the core extension methods which are designed to work with a more generic `IChangeToken` concept.
These core api's are in the `Dazinator.AspNetCore.Builder.ReloadablePipeline` nuget package.
You can use / run the middleware passing in a `Func<IChangeToken>` which can supply whatever custom change token you want to use to signal a pipeline rebuild - this could also be `CompositeChangeToken` if you have multiple sources.

You can also supply your own cancellation tokens for a pipeline, see the sample for a demonstration of that: https://github.com/dazinator/Dazinator.AspNetCore.Builder.ReloadablePipeline/blob/master/src/Sample/Startup.cs

