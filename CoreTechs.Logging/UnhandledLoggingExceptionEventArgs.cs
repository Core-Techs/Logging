using System;

namespace CoreTechs.Logging
{
    public class UnhandledLoggingExceptionEventArgs : EventArgs
    {
        public Exception Exception { get; private set; }

        public UnhandledLoggingExceptionEventArgs([NotNull] Exception exception)
        {
            if (exception == null) throw new ArgumentNullException("exception");
            Exception = exception;
        }
    }
}