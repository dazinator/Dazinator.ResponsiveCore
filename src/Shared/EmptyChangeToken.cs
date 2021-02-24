using System;
using Microsoft.Extensions.Primitives;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// A change token that never triggers.
    /// </summary>
    internal class EmptyChangeToken : IChangeToken
    {
        public static EmptyChangeToken Instance { get; } = new EmptyChangeToken();

        public bool HasChanged { get; } = false;
        public bool ActiveChangeCallbacks { get; } = true; // to prevent needless polling by consumers.

        public IDisposable RegisterChangeCallback(Action<object> callback, object state)
        {
            return EmptyDisposable.Instance;
        }
    }

}