using System;

namespace CoreTechs.Logging
{
    internal class GenericDisposable<T> : IDisposable
    {
        private readonly object _mutex = new object();
        private readonly Action<T> _onDispose;
        public T Value { get; }
        public bool Disposed { get; private set; }

        public GenericDisposable(T value, Action<T> onDispose)
        {
            Value = value;
            _onDispose = onDispose;
        }

        public void Dispose()
        {
            if (Disposed)
                return;

            lock (_mutex)
            {
                if (Disposed)
                    return;

                _onDispose?.Invoke(Value);

                Disposed = true;
            }
        }
    }
}