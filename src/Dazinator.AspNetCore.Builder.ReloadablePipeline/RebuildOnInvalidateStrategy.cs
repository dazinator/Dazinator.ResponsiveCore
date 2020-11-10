using System;
using Microsoft.AspNetCore.Http;

namespace Dazinator.AspNetCore.Builder.ReloadablePipeline
{
    /// <summary>
    /// Builds a new <see cref="RequestDelegate"/> in line with an Invalidate() call, within a lock, and then swaps it out for the next request. This means requests for the current RequestDelegate wont be interupted, and once the new one is available, the next request will return it, with no locks on get.
    /// </summary>
    public class RebuildOnInvalidateStrategy : IRebuildStrategy
    {
        private RequestDelegate _currentRequestDelegate = null;
        private readonly object _currentInstanceLock = new object();    
        private Func<RequestDelegate> _buildDelegate;

        public RebuildOnInvalidateStrategy()
        {
        }

        public void Initialise(Func<RequestDelegate> buildDelegate)
        {
            _buildDelegate = buildDelegate;
            Invalidate(); // cause intitial build now.
        }

        public void Invalidate()
        {
            // Only allow one build at a time.
            lock (_currentInstanceLock)
            {
                RequestDelegate newInstance = _buildDelegate();
                    //RequestDelegateUtils.BuildRequestDelegate(builder, onNext, _configure, _isTerminal);
                _currentRequestDelegate = newInstance;
            }

        }      

        public RequestDelegate Get()
        {
            return _currentRequestDelegate;     
        }
    }
}