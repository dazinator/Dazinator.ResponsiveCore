using System;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// A disposable that does nothing.
    /// </summary>
    internal class EmptyDisposable : IDisposable
    {
        public static EmptyDisposable Instance { get; } = new EmptyDisposable();

        private EmptyDisposable()
        {
        }

        public void Dispose()
        {
        }
    }

}