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
            return cancellationTokenFactory.Cast(token => new CancellationChangeToken(token));
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