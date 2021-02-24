using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace Dazinator.ResponsiveCore.ReloadablePipeline
{
    /// <summary>
    /// Builds a <see cref="RequestDelegate"/> on demand the first time its requested after it beign invalidated.
    /// </summary>
    public class RequestDelegateFactory : IRequestDelegateFactory
    {
        private readonly Func<RequestDelegate, RequestDelegate> _requestDelegateBuilder;
        private readonly IDisposable _listening = null;
        private readonly ReloadableMiddlewarePipelineOptions _options;

        public RequestDelegateFactory(
            ReloadableMiddlewarePipelineOptions options,
            Func<RequestDelegate, RequestDelegate> requestDelegateBuilder)
        {
            _requestDelegateBuilder = requestDelegateBuilder;
            _listening = ChangeTokenDebouncer.OnChangeDebounce(options.ChangeTokenProducer, InvokeChanged, delayInMilliseconds: 500);
            _options = options;
        }

        private void InvokeChanged()
        {
            _options.RebuildStrategy.Invalidate();
        }

        public Task<RequestDelegate> GetRequestDelegateTask()
        {
            return _options.RebuildStrategy.Get();
        }

        public void Dispose()
        {
            _listening?.Dispose();
            _options.ChangeTokenProducerLifetime?.Dispose();
        }
        public void Initialise(RequestDelegate onNext)
        {
            _options.RebuildStrategy.Initialise(() =>
            {
                return _requestDelegateBuilder?.Invoke(onNext);
            });
        }
    }
}