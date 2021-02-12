using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Dazinator.ResponsiveCore.ReloadablePipeline
{
    /// <summary>
    /// Lazily builds a <see cref="RequestDelegate"/> on demand the first time its requested, in-line with the caller.
    /// </summary>
    public class RebuildOnDemandStrategy : IRebuildStrategy
    {
        private Func<RequestDelegate> _buildDelegate;
        private Task<RequestDelegate> _currentResult;

        public void Invalidate()
        {
            // re-init the task that builds the pipeline.
            // but don't run it yet, as we want to defer building the pipeline until something asks for it.
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
            // or task will not be completed, and will need to be run, only once, by one of the conccurrent callers. 
            // Multiple callers (awaits) awaiting the same task instance is apparantly safe - the task will only execute once.
            return _currentResult;
        }
    }
}