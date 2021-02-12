## Features

`ResponsiveCore` provides features to enable your .net core applications to be more responsive to changes at runtime, so you don't have to restart the application in order for changes to take effect. 
It does this whilst trying to be as minimally invasive in your code base as possible.

Features:

- Middleware: Define middleware pipelines that can be dynamically reloaded / rebuilt from latest config at runtime, without dropping requests.
- Background Services: Allow your existing `IHostedService`s to be dynamically started and stopped based on your own logic to determine whether they should be currently enabled or not.
|
[![Build Status](https://dev.azure.com/darrelltunnell/Public%20Projects/_apis/build/status/dazinator.Dazinator.AspNetCore.Builder.ReloadablePipeline?branchName=develop)](https://dev.azure.com/darrelltunnell/Public%20Projects/_build/latest?definitionId=13&branchName=develop)

## Reloadable Middleware Pipelines

1. Add the `Dazinator.ResponsiveCore.ReloadablePipeline.Options` nuget package to your project.
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

### How do I signal the pipeline to rebuild for other sorts of changes - for example a button click?

The extension methods provided in `Dazinator.AspNetCore.Builder.ReloadablePipeline.Options` package is just a thin wrapper around the core extension methods which are designed to work with a more generic `IChangeToken` concept.
These core api's are in the `Dazinator.AspNetCore.Builder.ReloadablePipeline` nuget package.
You can use / run the middleware passing in a `Func<IChangeToken>` which can supply whatever custom change token you want to use to signal a pipeline rebuild - this could also be `CompositeChangeToken` if you have multiple sources.

You can also supply your own cancellation tokens for a pipeline, see the sample for a demonstration of that: https://github.com/dazinator/Dazinator.AspNetCore.Builder.ReloadablePipeline/blob/master/src/Sample/Startup.cs

### Rebuild Strategies

The default strategy for pipeline rebuilds, is to rebuild it within a lock at the point its invalidated. 
Once the new pipeline has been build, the new instance is swapped in for the old instance (no locking), and new requests are now pushed through the new instance of the pipeline instead of the old.
This strategy means there is no downtime for requests - requests continue to be processed through the old pipeline until the new one is swapped in without any lock delaying requests.
Another optional strategy that is provided out of the box, allows instead for the pipeline to be lazily re-built in line with the next request. This means other requests that occur whilst a rebuild is in progress may queue behind the lock until the rebuild completes, and for this reason - its not the default strategy - however it might prove useful if for example you'd like access to a current httpcontext (request) during the pipeline build itself - which is ordinarly not achievable.

To override the default strategy, pass in an `IRebuildStrategy` as the last optional parameter (you could also implement your own):

```
   app.UseReloadablePipeline<PipelineOptions>((builder, options) => ConfigureReloadablePipeline(builder, env, options), new RebuildOnDemandStrategy()); // will rebuild the pipeline in line with a request behind a lock.


```

## Enabled Hosted Services

Suppose you have an `IHostedService` that you want to be able to disable or enable at runtime.

When the service is "enabled" it should be running. When it is disabled, it should be stopped.

You can do this without any code changes to your service itself.

You don't have to use the `Options` pattern for this, but this is the pattern I'll promote for this example.

1. Add the `Dazinator.ResponsiveCore.EnabledHostedService.Options` package to your project.

2. Create an `Options` class that you can then bind to config to represent whether the service is enabled or disabled.

```csharp
    public class HostedServiceOptions
    {
        public bool Enabled { get; set; }
    }
```

and

```
services.Configure<HostedServiceOptions>(Configuration.GetSection("HostedService"));
```

3. Replace your call to `services.AddHostedService` with  `AddOptionsEnabledHostedService`:

```
 services.AddOptionsEnabledHostedService<YourHostedService, HostedServiceOptions>(
                       shouldBeRunning: options => options.Enabled);

```

4. Start your application with that config setting as `false`. Your service will not start. Whilst the application is running, change the config to be `true` and save that change. The change is detected and your service will now start. Again, whilst your application is running, change the config back to `false` - your service will be stopped. Repeat this ad infinitum - if you have the time, all the while basking in the glory of this responsivity.

### I don't want to use the Options pattern

You can use the `services.AddEnabledHostedService` method and its overloads which let you supply your own `IChangeToken`s for triggering when a change to the enablement state has been made, and your own `Func<bool>` that will be invoked for returning what that current status is. 
Based on these two things the service will respond accordingly.
           


