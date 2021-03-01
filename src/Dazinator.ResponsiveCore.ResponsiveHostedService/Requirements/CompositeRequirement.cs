using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Dazinator.ResponsiveCore.ResponsiveHostedService.Requirements
{
    /// <summary>
    /// Runs an inner list of checks one at a time in sequence, stopping at the first that fails.
    /// </summary>
    public class CompositeRequirement : IRequirement, IDisposable
    {
        private readonly IEnumerable<IRequirement> _checks;
        private bool _disposedValue;

        public CompositeRequirement(
            IEnumerable<IRequirement> checks)
        {
            _checks = checks;
        }

        public async Task<bool> IsSatisfied(CancellationToken cancellationToken)
        {
            bool result = true;
            foreach (var item in _checks)
            {
                result = result && await item.IsSatisfied(cancellationToken);
                if (!result)
                {
                    return false;
                }
            }

            return true;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    foreach (var item in _checks)
                    {
                        if (item is IDisposable disposable)
                        {
                            disposable?.Dispose();                           
                        }
                    }
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                _disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~CompositeShouldServiceBeRunningCheck()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}