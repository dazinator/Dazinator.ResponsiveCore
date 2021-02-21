using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Extensions.Primitives
{
    public class CompositeChangeTokenFactoryBuilder
    {
        public CompositeChangeTokenFactoryBuilder()
        {

        }

        public Func<IChangeToken> Build()
        {
            if (Factories == null || Factories.Count == 0)
            {
                return () => EmptyChangeToken.Instance;
            }

            if (Factories.Count == 1)
            {
                return Factories[0];
            }

            var factories = Factories.ToArray();
            return () =>
            {
                var tokens = new IChangeToken[factories.Length];
                for (int i = 0; i < factories.Length; i++)
                {
                    tokens[i] = factories[i].Invoke();
                }
                return new CompositeChangeToken(tokens);
            };
        }

        public List<Func<IChangeToken>> Factories { get; } = new List<Func<IChangeToken>>();


        /// <summary>
        /// Inlcude your own change token's in the composite. 
        /// If your <see cref="Func{IChangeToken}"/> at any point returns null, 
        /// then an <see cref="EmptyChangeToken"/> will be returned to the consumer,
        /// which is a Noop token to avoid null ref exceptions.
        /// </summary>
        /// <param name="trigger"></param>
        /// <returns></returns>
        public CompositeChangeTokenFactoryBuilder Include(Func<IChangeToken> changeToken, bool dispose = true)
        {
            IChangeToken currentToken = null;
            Func<IChangeToken> result = () =>
            {
                // consumer is asking for a new token, any previous token is dead.                 
                var previous = Interlocked.Exchange(ref currentToken, changeToken() ?? EmptyChangeToken.Instance);
                if (dispose && previous is IDisposable disposable)
                {
                    disposable?.Dispose();
                }
                return currentToken;
            };

            Factories.Add(result);
            return this;
        }

        /// <summary>
        /// Inlcude your own change token's in the composite that are generated
        /// from the supplied <see cref="Func{CancellationToken}"/> and signalled when the cancellation tokens are signalled.
        /// If your <see cref="Func{CancellationToken}"/> at any point returns null, 
        /// then an <see cref="EmptyChangeToken"/> will be returned to the consumer,
        /// which is a Noop token to avoid null ref exceptions.
        /// </summary>
        /// <param name="trigger"></param>
        /// <returns></returns>
        public CompositeChangeTokenFactoryBuilder IncludeCancellationTokens(Func<CancellationToken> cancellationTokenFactory, bool dispose = true)
        {

            var factory = cancellationTokenFactory.Cast(token =>
            {
                if (token == null)
                {
                    return EmptyChangeToken.Instance;
                }
                else
                {
                    return (IChangeToken)new CancellationChangeToken(token);
                }
            });

            IChangeToken currentToken = null;
            Func<IChangeToken> result = () =>
            {
                // consumer is asking for a new token, any previous token is dead.                 
                var previous = Interlocked.Exchange(ref currentToken, factory());
                if (dispose && previous is IDisposable disposable)
                {
                    disposable?.Dispose();
                }
                return currentToken;
            };

            Factories.Add(result);
            return this;
        }


        /// <summary>
        /// Inlcude a change token that can be manually triggered.
        /// </summary>
        /// <param name="trigger"></param>
        /// <returns></returns>
        public CompositeChangeTokenFactoryBuilder IncludeTrigger(out Action trigger)
        {
            TriggerChangeToken currentToken = null;
            Func<IChangeToken> result = () =>
            {
                // consumer is asking for a new token, any previous token is dead.                
                var previous = Interlocked.Exchange(ref currentToken, new TriggerChangeToken());
                previous?.Dispose();
                return currentToken; // another thread could have just swapped in a new value to this reference
                                     // in that case its preferable that we return the newer one, but if we do return the older one, 
                                     // its ok because it will be disposed by the later thread?
            };

            Factories.Add(result);
            trigger = () =>
            {
                currentToken?.Trigger();
            };

            return this;
        }

        /// <summary>
        /// Takes a callback that will be invoked when the first change token is requested,
        /// and will be provided an Action that can be invoked to invalidate the change token and all subsequent change tokens produced.
        /// </summary>
        /// <param name="registerListener"></param>
        /// <returns></returns>
        public CompositeChangeTokenFactoryBuilder IncludeDeferredTrigger(Action<Action> subscribeDelegate)
        {
            TriggerChangeToken currentToken = null;

            // lazy because we wait for a change token to be requested 
            // before we bother attaching the subscriber.
            Lazy<Action> activeSubscription = new Lazy<Action>(() =>
            {
                return () => subscribeDelegate(() => currentToken?.Trigger());
            });


            Func<IChangeToken> result = () =>
            {

                var previous = Interlocked.Exchange(ref currentToken, new TriggerChangeToken());
                previous?.Dispose();

                var newToken = currentToken;
                // consumer is asking for a new token
                // Ensure they have a callback to signal change tokens now.
                if (!activeSubscription.IsValueCreated)
                {
                    activeSubscription.Value.Invoke();
                }

                return newToken;
            };

            Factories.Add(result);
            return this;
        }

        /// <summary>
        /// Takes a callback that will be invoked when the first change token is requested,
        /// and will be provided an Action that can be invoked to invalidate the change token and all subsequent change tokens produced.
        /// </summary>
        /// <param name="registerListener"></param>
        /// <returns></returns>
        public CompositeChangeTokenFactoryBuilder IncludeDeferredAsyncTrigger(Func<Action, Task> callback)
        {
            TriggerChangeToken currentToken = null;

            // lazy because we wait for a change token to be requested 
            // before we bother attaching the subscriber.
            Lazy<Task> activeSubscription = new Lazy<Task>(async () =>
            {
                await callback(() => currentToken?.Trigger());
            });


            Func<IChangeToken> result = () =>
            {

                var previous = Interlocked.Exchange(ref currentToken, new TriggerChangeToken());
                previous?.Dispose();

                var newToken = currentToken;
                // consumer is asking for a new token
                // Ensure we have supplied the trigger action to signal tokens.
                if (!activeSubscription.IsValueCreated)
                {
                    activeSubscription.Value.Forget();
                }

                return newToken;
            };

            Factories.Add(result);
            return this;
        }

        /// <summary>
        /// Takes a function that will register a "listener" / trigger <see cref="Action"/> and return a <see cref="IDisposable"/> to represent the active subscription,
        /// and adds a change token factory whose <see cref="IChangeToken"/> will be signalled whenever the trigger <see cref="Action"/> is invoked.
        /// The function to register the trigger <see cref="Action"/> is lazily invoked when the first <see cref="IChangeToken"/> is requested, and then the returned <see cref="IDisposable"/> 
        /// is kept which represents the registration. 
        /// </summary>
        /// <param name="registerListener"></param>
        /// <returns></returns>
        public CompositeChangeTokenFactoryBuilder IncludeSubscribingHandlerTrigger(Func<Action, IDisposable> subscribeDelegate)
        {
            TriggerChangeToken currentToken = null;

            // lazy because we wait for a change token to be requested 
            // before we bother attaching the subscriber.
            Lazy<IDisposable> activeSubscription = new Lazy<IDisposable>(() =>
            {
                return subscribeDelegate(() => currentToken?.Trigger());
            });


            Func<IChangeToken> result = () =>
            {
                // consumer is asking for a new token
                // Ensure we are actively listening for callbacks.
                if (!activeSubscription.IsValueCreated)
                {
                    _ = activeSubscription.Value;
                }

                var previous = Interlocked.Exchange(ref currentToken, new TriggerChangeToken());
                previous?.Dispose();
                return currentToken;
            };

            Factories.Add(result);
            return this;
        }


        /// <summary>
        /// Takes a registration function that will be invoked to register a "listener" / trigger <see cref="Action"/> and return a <see cref="IDisposable"/> to represent the active subscription,
        /// and adds a change token factory whose <see cref="IChangeToken"/> will be signalled whenever the trigger <see cref="Action"/> is invoked.
        /// The function to register the trigger <see cref="Action"/> is lazily invoked when the first <see cref="IChangeToken"/> is requested, and then the returned <see cref="IDisposable"/> 
        /// is kept which represents the registration. When the action is triggered, not only will the change token be signalled, but the subscription will be disposed, and the registration function will then be invoked again to 
        /// create a new subscription of the listener.
        /// </summary>
        /// <param name="registerListener"></param>
        /// <returns></returns>
        public CompositeChangeTokenFactoryBuilder IncludeResubscribingHandlerTrigger(Func<Action, IDisposable> registerListener)
        {

            TriggerChangeToken currentToken = null;
            IDisposable registration = null;

            Func<IChangeToken> result = () =>
            {
                // consumer is asking for a new token, initialise it first so that if the registerListener callback below happens immeditely it will trigger the new token
                // not the old. This does also mean there is a period of time in which if the current listener fires again it will trigger the new token but think thats ok.
                var previousToken = Interlocked.Exchange(ref currentToken, new TriggerChangeToken());
                previousToken?.Dispose();

                // Ensure we are actively listening for callbacks, adding the new subscription first,
                // before disposing any old subscription to ensure no gaps in listener coverage.
                var previousRegistration = Interlocked.Exchange(ref registration, registerListener(() => currentToken?.Trigger()));
                previousRegistration?.Dispose();

                return currentToken;
            };

            Factories.Add(result);
            return this;
        }


        /// <summary>
        /// Include a change token that will be triggered whenever an event handler is invoked.
        /// A callback is called when the first change token is requested, for you to register the event handler.
        /// A disposable is also provided via a callback, and should be disposed to remove the event handler and stop listening for changes.
        /// </summary>
        public CompositeChangeTokenFactoryBuilder IncludeEventHandlerTrigger<TEventArgs>(
            Action<EventHandler<TEventArgs>> addHandler,
            Action<EventHandler<TEventArgs>> removeHandler,
            Action<IDisposable> handlerLifetime)
        {
            TriggerChangeToken currentToken = null;
            EventHandler<TEventArgs> triggerChangeTokenHandler = (a, e) =>
            {
                currentToken?.Trigger();
            };

            Lazy<IDisposable> subscription = new Lazy<IDisposable>(() => {

                addHandler(triggerChangeTokenHandler);
                return new InvokeOnDispose(() =>
                {
                    removeHandler(triggerChangeTokenHandler);
                });
            });

            Func<IChangeToken> factory = () =>
            {
                // deliberately capture a reference to the disposable subscription representing the event handler.
                // lazy initialise it when first token requested - will attach the event handler.
                // When this factory falls out of scope, the reference to the IDisposable is lost

                // and gets collected.
                if(!subscription.IsValueCreated)
                {
                    _ = subscription.Value;
                    handlerLifetime(subscription.Value);
                }
                // consumer is asking for a new token, any previous token is dead.                
                var previous = Interlocked.Exchange(ref currentToken, new TriggerChangeToken());
                previous?.Dispose();
                return currentToken;
            };
           
            Factories.Add(factory);
            return this;
        }


        // IncludeResubscribingTrigger

        // .Include(()=>new MyChangeToken())      
        // .IncludeTrigger(out Action trigger)
        // .IncludeDefferedTrigger((trigger) => trigger())
        // .IncludeDeferredAsyncTrigger(async (trigger) => { 
        //    await Task.Delay(4000);
        //    trigger();
        //    await Task.Delay(5000);
        //    trigger();
        // })
        // .IncludeSubscriberTrigger((a)=> { return foo.OnChange(a); } // single subscription to OnChange kept alive, always listening.
        // .IncludeResubscriberTrigger((a)=> { IDisposable subscription = foo.OnChange(a); 
        //                                     a = a + 1; // will keep incrementing because each time token is singalled,
        //                                                this method is fired to resubscribe and return a new subscription - old one is disposed. 
        //                                     return subscription;
        //                                   });
        // .IncludeOptionsMonitor<MyOptions>()
        // .IncludeSubscriberDelegate((a)=> s.OnChange(a))
        // .IncludeEventHandler<MyEventArgs>(
        //                 addHandler: (a)=> this.MyEvent += a,
        //                 removeHandler: (a)=> this.MyEvent -= a,
        //                 out IDisposable subscribed)
        // .Build();

    }

}