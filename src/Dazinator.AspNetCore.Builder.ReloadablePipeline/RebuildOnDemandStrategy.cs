using System;
using Microsoft.AspNetCore.Http;

namespace Dazinator.AspNetCore.Builder.ReloadablePipeline
{
    /// <summary>
    /// Builds a <see cref="RequestDelegate"/> on demand the first time its requested, in-line with the request, behind a lock.
    /// </summary>
    public class RebuildOnDemandStrategy : IRebuildStrategy
    {
        private RequestDelegate _currentRequestDelegate = null;
        private readonly object _currentInstanceLock = new object();
        private Func<RequestDelegate> _buildDelegate;

        public RebuildOnDemandStrategy()
        {
        }

        public void Invalidate()
        {
            _currentRequestDelegate = null;
        }

        public void Initialise(Func<RequestDelegate> buildDelegate)
        {
            _buildDelegate = buildDelegate;
        }

        public RequestDelegate Get()
        {
            var existing = _currentRequestDelegate;
            if (existing != null)
            {
                return existing;
            }

            // Only allow one build at a time.
            lock (_currentInstanceLock)
            {
                if (existing != null)
                {
                    return existing;
                }

                var newInstance = _buildDelegate();
              //  RequestDelegate newInstance = RequestDelegateUtils.BuildRequestDelegate(builder, onNext, _configure, _isTerminal);
                _currentRequestDelegate = newInstance;
                // as we don't lock in Invalidate(), it could have just set _currentRequestDelegate back to null here,
                // that's why we keep hold of and return, newInstance - as this method must always return an instance to satisfy current request.
                return newInstance;
            }

        }     
    }
}