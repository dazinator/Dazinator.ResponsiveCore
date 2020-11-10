using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace Dazinator.AspNetCore.Builder.ReloadablePipeline
{
    public class ReloadPipelineMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IRequestDelegateFactory _factory;
        public ReloadPipelineMiddleware(
            RequestDelegate next,
            IApplicationBuilder rootBuilder,
            IRequestDelegateFactory factory)
        {
            _next = next;
            factory.Initialise(_next);
            _factory = factory;          
        }

        public async Task Invoke(HttpContext context)
        {
            var requestDelegate = _factory.Get();
            await requestDelegate.Invoke(context);
        }
    }
}