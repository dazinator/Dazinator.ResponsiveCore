using System;
using System.Threading;

namespace Microsoft.Extensions.Primitives
{
    public static class ChangeTokenFactoryHelper
    {
        /// <summary>
        /// Adapts a <see cref="CancellationToken"/> factory, to a <see cref="IChangeToken"/> factory. When the <see cref="CancellationToken"/> is signalled, the <see cref="IChangeToken"/> is signalled.
        /// </summary>
        /// <param name="cancellationTokenFactory"></param>
        /// <returns></returns>
        public static Func<IChangeToken> UseCancellationTokens(Func<CancellationToken> cancellationTokenFactory)
        {           
            return cancellationTokenFactory.Convert(token => new CancellationChangeToken(token));
        }

        /// <summary>
        /// Takes a function that can register an action delegate as a "listener"and returns an <see cref="IDisposable"/> to represet that registration,
        /// and adapts it to function that returns an <see cref="IChangeToken"/> that will be signalled when the listener is invoked.
        /// When the <see cref="IChangeToken"/> is created, it registers the action delegate as a new "listener", disposing of any previous registration of that delegate first.
        /// </summary>
        /// <param name="registerListener"></param>
        /// <returns></returns>
        public static Func<IChangeToken> UseCallbackRegistrations(Func<Action, IDisposable> registerListener)
        {

            IDisposable previousRegistration = null;
            Func<IChangeToken> changeTokenFactory = () =>
            {
                // When should ensure any previous CancellationTokenSource is disposed, 
                // and we remove old monitor OnChange listener, before creating new ones.
                previousRegistration?.Dispose();

                var changeTokenSource = new CancellationTokenSource();
                var disposable = registerListener(() => changeTokenSource.Cancel());

                previousRegistration = new InvokeOnDispose(() =>
                {
                    // Ensure disposal of listener and token source that we created.
                    disposable.Dispose();
                    changeTokenSource.Dispose();
                });

                var changeToken = new CancellationChangeToken(changeTokenSource.Token);
                return changeToken;

            };

            return changeTokenFactory;

            //var changeTokenFactory = FuncUtils.KeepLatest<CancellationTokenSource, IDisposable>(
            //  (source) =>
            //  {
            //      // trigger cancellation when the registered listener is invoked.
            //      // the disposable represents the registration of the listener, disposing it removes the registration which stops it from listening.
            //      var disposable = registerListener(() => source.Cancel());
            //      // Composite dispose of this registration, and the cancellation token when we get disposed.
            //      var newDisposable = new InvokeOnDispose(() =>
            //     {
            //         // Ensure disposal of listener and token source that we created.
            //         disposable.Dispose();
            //         source.Dispose();
            //     });
            //      return newDisposable;
            //  })
            //  .ToAction()
            //  .ToFunc(new CancellationTokenSource(),
            //          (cts) => new CancellationChangeToken(cts.Token));

            //return changeTokenFactory;
        }

    }



}