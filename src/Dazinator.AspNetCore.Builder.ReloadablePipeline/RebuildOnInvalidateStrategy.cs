using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Dazinator.AspNetCore.Builder.ReloadablePipeline
{
    /// <summary>
    /// Builds a new <see cref="RequestDelegate"/> in line with an Invalidate() call, within a lock, and then swaps it out for the next request. This means requests for the current RequestDelegate wont be interupted, and once the new one is available, the next request will return it, with no locks on get.
    /// </summary>
    public class RebuildOnInvalidateStrategy : IRebuildStrategy
    {
        private Task<RequestDelegate> _currentResult = null;
        private readonly object _currentInstanceLock = new object();    
        private Func<RequestDelegate> _buildDelegate;      

        public void Initialise(Func<RequestDelegate> buildDelegate)
        {
            _buildDelegate = buildDelegate;

            // only want one build at a time, ideally this Initialise() method
            // will not be called concurrently anyway
            // but in case it is, let's make it safe.
            lock(_currentInstanceLock)
            {
                var newInstance = buildDelegate();              
                _currentResult = Task.FromResult(newInstance);
            }                   
        }

        public void Invalidate()
        {
            // re-init the task so it returns a new build of the pipeline.
            Initialise(_buildDelegate);       
        }      

        public Task<RequestDelegate> Get()
        {
            return _currentResult;     
        }
    }
}