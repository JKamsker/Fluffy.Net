using System;

namespace Fluffy.Utilities
{
    public static class DisposableFactory
    {
        public static IDisposable FromDelegates(Action onDispose)
        {
            return new DisposableObject(onDispose);
        }

        private class DisposableObject : IDisposable
        {
            private readonly Action _onDispose;
            private bool _disposed;

            public DisposableObject(Action onDispose)
            {
                _onDispose = onDispose;
            }

            public void Dispose()
            {
                if (_disposed)
                {
                    return;
                }
                _disposed = true;
                _onDispose?.Invoke();
            }
        }
    }
}