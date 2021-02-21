using System;

namespace Microsoft.Extensions.Primitives
{
    /// <summary>
    /// A change token that never triggers.
    /// </summary>
    public class EmptyChangeToken : IChangeToken
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