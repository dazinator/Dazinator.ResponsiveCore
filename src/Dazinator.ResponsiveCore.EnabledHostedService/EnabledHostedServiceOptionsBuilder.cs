using System;
using System.Linq.Expressions;
using Microsoft.Extensions.Primitives;

namespace Microsoft.Extensions.DependencyInjection
{
    public class EnabledHostedServiceOptionsBuilder<THostedService> : IEnabledHostedServiceOptionsBuilder<THostedService>
    {
        public EnabledHostedServiceOptionsBuilder()
        {
            ServiceResolver = ActivatorUtilities.GetServiceOrCreateInstance<THostedService>;
            ChangeTokenFactoryResolver = (sp) => () => EmptyChangeToken.Instance;
        }

        public Func<IServiceProvider, THostedService> ServiceResolver { get; set; }
        public Func<IServiceProvider, Func<IChangeToken>> ChangeTokenFactoryResolver { get; set; }
        public Func<IServiceProvider, Func<bool>> IsEnabledDelegateResolver { get; set; }
        public IDisposable Lifetime { get; set; } = EmptyDisposable.Instance;

        /// <summary>
        /// Use a factory Func to create the instance of the IHosted service that will be controlled by the enabled / disabled check. If you don't set this, by default <see cref="ActivatorUtilities.GetServiceOrCreateInstance<typeparamref name="THostedService"/>"/> is used.
        /// </summary>
        /// <param name="resolver"></param>
        /// <returns></returns>
        public IEnabledHostedServiceOptionsBuilder<THostedService> UseServiceFactory(Func<IServiceProvider, THostedService> resolver)
        {
            ServiceResolver = resolver;
            return this;
        }

        /// <summary>
        /// Use a factory Func to create the instance of the IHosted service that will be controlled by the enabled / disabled check. If you don't set this, by default <see cref="ActivatorUtilities.GetServiceOrCreateInstance<typeparamref name="THostedService"/>"/> is used.
        /// </summary>
        /// <param name="resolver"></param>
        /// <returns></returns>
        public IEnabledHostedServiceOptionsBuilder<THostedService> UseServiceFactory(Func<THostedService> resolver)
        {
            ServiceResolver = (s) => resolver();
            return this;
        }


        /// <summary>
        /// Set the factory that produces <see cref = "IChangeToken" /></ param > s, used to signal when the service needs to re-check it's latest enabled / disabled status.
        /// </summary>
        /// <param name="resolver"></param>
        /// <returns></returns>
        public IEnabledHostedServiceOptionsBuilder<THostedService> UseChangeTokenFactory(Func<IServiceProvider, Func<IChangeToken>> resolver)
        {
            ChangeTokenFactoryResolver = resolver;
            return this;
        }

        /// <summary>
        /// Factory that produces <see cref = "IChangeToken" /></ param > s, used to signal when the service needs to re-check it's latest enabled / disabled status.
        /// </summary>
        /// <param name="resolver"></param>
        /// <returns></returns>
        public IEnabledHostedServiceOptionsBuilder<THostedService> UseChangeTokenFactory(Func<IChangeToken> resolver)
        {
            ChangeTokenFactoryResolver = s => resolver;
            return this;
        }

        public IEnabledHostedServiceOptionsBuilder<THostedService> UseChangeTokenFactory(Action<ChangeTokenProducerBuilder> configure)
        {
            var builder = new ChangeTokenProducerBuilder();
            configure(builder);
            ChangeTokenFactoryResolver = s => {
                var result = builder.Build(out IDisposable disposable);
                Lifetime = disposable;
                return result;
            };            
            return this;
        }

        public IEnabledHostedServiceOptionsBuilder<THostedService> UseChangeTokenFactory(Action<IServiceProvider, ChangeTokenProducerBuilder> configure)
        {
            var builder = new ChangeTokenProducerBuilder();
            ChangeTokenFactoryResolver = s =>
            {
                configure(s, builder);
                var result = builder.Build(out var disposable);
                Lifetime = disposable;
                return result;
            };
            return this;
        }


        /// <summary>
        /// Set the delegate to be used to check the status of the service. The delegate returns the desired enablement state for the service - i.e whether the service should be enabled or not. 
        /// If the delegate returns true, it means the service should be enabled. If it is not currently running it will be started.
        /// If the delegate returns false, it means the service snould not be enabled. If it is currently running it will be stopped.
        /// </summary>
        /// <param name="resolver"></param>
        /// <returns></returns>
        public IEnabledHostedServiceOptionsBuilder<THostedService> UseEnabledChecker(Func<IServiceProvider, Func<bool>> resolver)
        {
            IsEnabledDelegateResolver = resolver;
            return this;
        }

        /// <summary>
        /// Set the delegate to be used to check the status of the service. The delegate returns the desired enablement state for the service - i.e whether the service should be enabled or not. 
        /// If the delegate returns true, it means the service should be enabled. If it is not currently running it will be started.
        /// If the delegate returns false, it means the service snould not be enabled. If it is currently running it will be stopped.
        /// </summary>
        /// <param name="resolver"></param>
        /// <returns></returns>
        public IEnabledHostedServiceOptionsBuilder<THostedService> UseEnabledChecker(Func<bool> resolver)
        {
            IsEnabledDelegateResolver = s => resolver;
            return this;
        }

        public IEnabledHostedServiceOptionsBuilder<THostedService> UseEnabledChecker(Action<IFuncBoolBuilderInitial> buildCallback)
        {
            var checkFunc = FuncBoolBuilder.Build((builder) => {
                buildCallback(builder);
            });

            IsEnabledDelegateResolver = s => checkFunc;
            return this;
        }
        public IEnabledHostedServiceOptionsBuilder<THostedService> UseEnabledChecker(Action<IServiceProvider, IFuncBoolBuilderInitial> buildCallback)
        {
            IsEnabledDelegateResolver = s => {
                var checkFunc = FuncBoolBuilder.Build((builder) => {
                    buildCallback(s, builder);
                });
                return checkFunc;
            };
            return this;
        }
    }


}