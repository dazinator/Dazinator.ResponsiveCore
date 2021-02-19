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
        public static Func<IChangeToken> CreateChangeTokenFactory(this Func<CancellationToken> cancellationTokenFactory)
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
        public static Func<IChangeToken> CreateChangeTokenFactory(Func<Action, IDisposable> registerListener)
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
        }

        /// <summary>
        /// Creates a factory to produce change tokens that are signalled whenever an event handler is invoked.
        /// </summary>
        /// <returns>An <see cref="IDisposable"/></returns> that represents the event handler subscription. Dispose of it to unsubscribe the event handler from the event, which will then cause change tokens to be signalled no longer when the event is raised in future.
        public static Func<IChangeToken> CreateChangeTokenFactoryUsingEventHandler<TEventArgs>(Action<EventHandler<TEventArgs>> addHandler, Action<EventHandler<TEventArgs>> removeHandler, out IDisposable subscription)
        {
            TriggerChangeToken currentToken = null;
            Func<IChangeToken> result = () =>
              {
                  // consumer is asking for a new token, any previous token is dead.                
                  var previous = Interlocked.Exchange(ref currentToken, new TriggerChangeToken());
                  previous?.Dispose();
                  return currentToken;
              };

            EventHandler<TEventArgs> triggerChangeTokenHandler = (a, e) =>
             {
                 currentToken?.Trigger();
             };
            addHandler(triggerChangeTokenHandler);
            //eventHandler += triggerChangeTokenHandler;
            subscription = new InvokeOnDispose(() =>
            {
                removeHandler(triggerChangeTokenHandler);
                //  eventHandler -= triggerChangeTokenHandler;

            });

            return result;
        }
    }

}