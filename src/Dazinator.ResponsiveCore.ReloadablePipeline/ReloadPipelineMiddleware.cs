using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Dazinator.ResponsiveCore.ReloadablePipeline
{
    public class ReloadPipelineMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IRequestDelegateFactory _factory;
        public ReloadPipelineMiddleware(
            RequestDelegate next,
            IRequestDelegateFactory factory)
        {
            _next = next;
            factory.Initialise(_next);
            _factory = factory;
        }

        public async Task Invoke(HttpContext context)
        {
            var requestDelegateTask = _factory.GetRequestDelegateTask();
            // RE: awaiting the task below..
            // Hot path: is to await an already completed task (if pipeline has been built which is normal case) as there is no work to be done.
            //           - Note: i'm very much hoping that awaiting a completed task is optimised by the runtime for basically no overhead..?
            // Cool path: The task returned represents some work that needs to be done to build the pipeline in line with this request (if using such an IRebuildStrategy)
            //           - Note: I'm then relying on the fact that this request (and any concurrent request that has reached here) will asynchronously await the same task,
            //           which should result in the task only running once - and its result being made available to all concurrent callers. This avoids having to explicitly place any lock around the rebuild operation.
            var requestDelegate = await requestDelegateTask;
            await requestDelegate.Invoke(context);
        }
    }
}