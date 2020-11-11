using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Dazinator.AspNetCore.Builder.ReloadablePipeline
{
    /// <summary>
    /// Builds a <see cref="RequestDelegate"/> on demand the first time its requested, in-line with the request, behind a lock.
    /// </summary>
    public class RebuildOnDemandStrategy : IRebuildStrategy
    {
        private Func<RequestDelegate> _buildDelegate;
        private Task<RequestDelegate> _currentResult;
      
        public void Invalidate()
        {
            // re-init the task that builds the pipeline.
            Initialise(_buildDelegate);
        }

        public void Initialise(Func<RequestDelegate> buildDelegate)
        {
            _buildDelegate = buildDelegate;
            _currentResult = new Task<RequestDelegate>(_buildDelegate);
        }

        public Task<RequestDelegate> Get()
        {
            // concurrent threads will obtain the same task instance
            // this task will either be already completed (if previously awaited and pipeline already built)
            // or task will be run by one of the conccurrent threads. 
            // Multiple threads awaiting the same task is safe - the task will only run on one thread at a time.
            return _currentResult;       
        }
    }
}