using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace Dazinator.ResponsiveCore.ReloadablePipeline
{
    /// <summary>
    /// Builds a <see cref="RequestDelegate"/> on demand the first time its requested after it beign invalidated.
    /// </summary>
    public class RequestDelegateFactory : IRequestDelegateFactory
    {
        private readonly IRebuildStrategy _rebuildStrategy;
        private readonly Func<RequestDelegate, RequestDelegate> _requestDelegateBuilder;
        private readonly IDisposable _listening = null;

        public RequestDelegateFactory(
            Func<IChangeToken> getNewChangeToken,
            IRebuildStrategy rebuildStrategy,
            Func<RequestDelegate, RequestDelegate> requestDelegateBuilder)
        {
            _rebuildStrategy = rebuildStrategy;
            _requestDelegateBuilder = requestDelegateBuilder;
            _listening = ChangeTokenHelper.OnChangeDebounce(getNewChangeToken, InvokeChanged, delayInMilliseconds: 500);
        }

        private void InvokeChanged()
        {
            _rebuildStrategy.Invalidate();
        }

        public Task<RequestDelegate> GetRequestDelegateTask()
        {
            return _rebuildStrategy.Get();
        }

        public void Dispose() => _listening?.Dispose();
        public void Initialise(RequestDelegate onNext)
        {
            _rebuildStrategy.Initialise(() =>
            {
                return _requestDelegateBuilder(onNext);
            });
        }
    }
}