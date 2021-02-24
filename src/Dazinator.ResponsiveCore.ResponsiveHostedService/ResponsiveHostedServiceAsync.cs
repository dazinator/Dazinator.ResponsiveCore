using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Primitives;

namespace Dazinator.ResponsiveCore.ResponsiveHostedService
{
    public class ResponsiveHostedServiceAsync<TInner> : IHostedService, IDisposable
where TInner : IHostedService
    {
        private readonly TInner _inner;
        private readonly ResponsiveHostedServiceOptions _options;
        //private readonly Func<IChangeToken> _changeTokenFactory;
        //private readonly IDisposable _tokenFactoryLifetime;
        //private readonly Func<CancellationToken, Task<bool>> _shouldBeRunning;
        private IDisposable _listening;

        private bool _isRunning;
        private bool _disposedValue;

        // this service decorates an inner service,
        // but allows it to be started or stopped at runtime
        // based on a change token that gets triggered, to re-evaluate whether
        // it should be currently running or not. Based on that evaluation and the current state of the service,
        // StartAsync or StopAsync will be called.
        public ResponsiveHostedServiceAsync(
            TInner inner,
            ResponsiveHostedServiceOptions options)
        {
            _inner = inner;
            _options = options;
            _listening = ChangeTokenDebouncer.OnChangeDebounce(_options.ChangeTokenProducer, InvokeChanged, delayInMilliseconds: 500);
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await EnsureInnerAsync(cancellationToken);
        }

        private void InvokeChanged()
        {
            // We don't wait for the task to complete here because we would have to block whilst the service
            // potentially starts or stops.
            EnsureInnerAsync().Forget();
        }

        public async Task EnsureInnerAsync(CancellationToken hostCancellationToken = default)
        {
            // check what the desired state for this service is
            // by invoking a predicate - should it be running or not?
            // then enact that desired state by comparing against the IsRunning and starting or stopping accordingy.

            bool shouldBeRunning = await _options.ShouldBeRunningAsyncCheck(hostCancellationToken);
            // TODO: may need to lock in case Start or Stop is currently running and hasn't finished toggling the _isRunning flag yet.
            if (_isRunning && !shouldBeRunning)
            {
                var token = default(CancellationToken); // todo: should probably use a proper token here with a time specified.
                await StopInnerAsync(token);
            }
            else if (!_isRunning && shouldBeRunning)
            {
                var token = default(CancellationToken); // todo: should probably use a proper token here with a time specified.
                await StartInnerAsync(token);
            }
        }

        public async Task StopInnerAsync(CancellationToken cancellationToken)
        {
            if (_isRunning)
            {
                await _inner.StopAsync(cancellationToken);
                _isRunning = false;
            }
        }

        public async Task StartInnerAsync(CancellationToken cancellationToken)
        {
            if (!_isRunning)
            {
                await _inner.StartAsync(cancellationToken);
                _isRunning = true;
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            var token = cancellationToken;
            await StopInnerAsync(token);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                    _listening?.Dispose();
                    _options.ChangeTokenProducerLifetime?.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }

}