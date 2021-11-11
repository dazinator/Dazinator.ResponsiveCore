using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;

namespace Dazinator.ResponsiveCore.ResponsiveHostedService
{
    public class ResponsiveHostedServiceAsync<TInner> : IHostedService, IDisposable
        #if SUPPORTS_ASYNC_DISPOSABLE
        ,IAsyncDisposable
        #endif
where TInner : IHostedService
    {
        private readonly ILogger<ResponsiveHostedService.ResponsiveHostedServiceAsync<TInner>> _logger;
        private readonly TInner _inner;
        private readonly ResponsiveHostedServiceOptions _options;
        private IDisposable _listening;

        private bool _isRunning;
        private bool _disposedValue;

        // this service decorates an inner service,
        // but allows it to be started or stopped at runtime
        // based on a change token that gets triggered, to re-evaluate whether
        // it should be currently running or not. Based on that evaluation and the current state of the service,
        // StartAsync or StopAsync will be called.
        public ResponsiveHostedServiceAsync(
            ILogger<ResponsiveHostedServiceAsync<TInner>> logger,
            TInner inner,
            ResponsiveHostedServiceOptions options)
        {
            _logger = logger;
            _inner = inner;
            _options = options;
            _listening = ChangeTokenDebouncer.OnChangeDebounce(_options.ChangeTokenProducer, InvokeChanged, delayInMilliseconds: options.DebounceDelayInMilliseconds);
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

            _logger.LogDebug("Checking whether service should be running or not.");
            bool shouldBeRunning = await _options.ShouldBeRunningAsyncCheck(hostCancellationToken);
            // TODO: may need to lock in case Start or Stop is currently running and hasn't finished toggling the _isRunning flag yet.
            if (_isRunning && !shouldBeRunning)
            {
                var token = default(CancellationToken); // todo: should probably use a proper token here with a time specified.
                await StopInnerAsync(token);
            }
            else if (!_isRunning && shouldBeRunning)
            {
                _logger.LogDebug("Service should be started.");
                var token = default(CancellationToken); // todo: should probably use a proper token here with a time specified.
                await StartInnerAsync(token);
            }
        }

        public async Task StopInnerAsync(CancellationToken cancellationToken)
        {
            if (_isRunning)
            {
                _logger.LogDebug("Stopping.");
                // stopping
                await _inner.StopAsync(cancellationToken);
                _isRunning = false;
            }
            else
            {
                _logger.LogDebug("Service already stopped.");
            }
        }

        public async Task StartInnerAsync(CancellationToken cancellationToken)
        {
            if (!_isRunning)
            {
                _logger.LogDebug("Starting");
                // starting
                await _inner.StartAsync(cancellationToken);
                _isRunning = true;
            }
            else
            {
                _logger.LogDebug("Service already started.");
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            var token = cancellationToken;
            await StopInnerAsync(token);
        }

       
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

#if SUPPORTS_ASYNC_DISPOSABLE

        public async ValueTask DisposeAsync()
        {
            await DisposeAsyncCore();

            Dispose(disposing: false);
#pragma warning disable CA1816 // Dispose methods should call SuppressFinalize
            GC.SuppressFinalize(this);
#pragma warning restore CA1816 // Dispose methods should call SuppressFinalize
        }


        protected virtual async ValueTask DisposeAsyncCore()
        {
          
            if (_listening is IAsyncDisposable disposable)
            {
                await disposable.DisposeAsync().ConfigureAwait(false);
            }
            else
            {
                _listening?.Dispose();
            }

            if (_inner is IAsyncDisposable innerAsyncDisposable)
            {
                await innerAsyncDisposable.DisposeAsync().ConfigureAwait(false);
            }
            else
            {
                if (_inner is IDisposable innerDisposable)
                {
                    innerDisposable?.Dispose();
                }                
            }

            _listening = null;          
        }


#endif

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _logger.LogDebug("Disposing");
                    // TODO: dispose managed state (managed objects)
                    _listening?.Dispose();
                    _options.ChangeTokenProducerLifetime?.Dispose();
                    if (_inner is IDisposable inner)
                    {
                        _logger.LogDebug("Disposing service");
                        inner?.Dispose();
                        _logger.LogDebug("Service disposed.");

                    }
                    else
                    {
                        _logger.LogDebug("Service is not IDisposable.");
                    }
                }

                _listening = null;
                _disposedValue = true;
            }

        }


    }

}