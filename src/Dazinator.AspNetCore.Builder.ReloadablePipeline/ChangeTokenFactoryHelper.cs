using System;
using System.Threading;
using Dazinator.AspNetCore.Builder.ReloadablePipeline.Utils;
using Microsoft.Extensions.Primitives;

namespace Microsoft.AspNetCore.Builder
{
    public static class ChangeTokenFactoryHelper
    {
        public static Func<IChangeToken> UseCancellationTokens(Func<CancellationToken> cancellationTokenFactory)
        {
            Func<IChangeToken> changeTokenFactory = () =>
            {
                var token = cancellationTokenFactory();
                var changeToken = new CancellationChangeToken(token);
                return changeToken;
            };
            return changeTokenFactory;
        }

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
        }

    }
}
