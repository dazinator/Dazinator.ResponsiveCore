using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace Dazinator.AspNetCore.Builder.ReloadablePipeline
{
    /// <summary>
    /// Builds a <see cref="RequestDelegate"/> on demand the first time its requested after it beign invalidated.
    /// </summary>
    public class RequestDelegateFactory : IRequestDelegateFactory
    {
        private readonly IRebuildStrategy _rebuildStrategy;
        private readonly Func<RequestDelegate, RequestDelegate> _requestDelegateBuilder;

        //  private readonly IApplicationBuilder _appBuilder;
        private readonly IDisposable _listening = null;
       // public IApplicationBuilder _subBuilder { get; set; }

        public RequestDelegateFactory(
            Func<IChangeToken> getNewChangeToken,
            IRebuildStrategy rebuildStrategy, 
            Func<RequestDelegate, RequestDelegate> requestDelegateBuilder)
        {
            _rebuildStrategy = rebuildStrategy;
            _requestDelegateBuilder = requestDelegateBuilder;
            //  _appBuilder = appBuilder;
            _listening = ChangeTokenHelper.OnChangeDebounce(getNewChangeToken, InvokeChanged, delayInMilliseconds: 500);
        }

        private void InvokeChanged()
        {
            _rebuildStrategy.Invalidate();
        }

        public RequestDelegate Get()
        {
            return _rebuildStrategy.Get();
        }

        public void Dispose() => _listening?.Dispose();
        public void Initialise(RequestDelegate onNext)
        {           
            _rebuildStrategy.Initialise(() => {
                return _requestDelegateBuilder(onNext);
            });
        }
        // public RequestDelegate Get() => throw new NotImplementedException();
    }   


}