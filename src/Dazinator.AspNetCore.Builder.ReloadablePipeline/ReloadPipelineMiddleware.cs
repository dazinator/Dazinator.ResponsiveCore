using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace Dazinator.AspNetCore.Builder.ReloadablePipeline
{
    public class ReloadPipelineMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly RequestDelegateFactory _factory;
        private readonly IApplicationBuilder _rootBuilder;
        private readonly bool _isTerminal;

        public ReloadPipelineMiddleware(
            RequestDelegate next,            
            IApplicationBuilder rootBuilder, 
            RequestDelegateFactory factory, bool isTerminal)
        {
            _next = next;
            _factory = factory;
            _rootBuilder = rootBuilder;
            _isTerminal = isTerminal;
        }

        public async Task Invoke(HttpContext context)
        {
            var requestDelegate = _factory.Get(_rootBuilder, _next, _isTerminal);
            await requestDelegate.Invoke(context);
        }
    }
}